using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para iniciar una partida individual
/// </summary>
public class StartSoloGameUseCase
{
    private readonly ISoloGameRepository _soloGameRepository;
    private readonly IEnergyRepository _energyRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IWorldRepository _worldRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;

    public StartSoloGameUseCase(
        ISoloGameRepository soloGameRepository,
        IEnergyRepository energyRepository,
        ILevelRepository levelRepository,
        IWorldRepository worldRepository,
        GetQuestionsUseCase getQuestionsUseCase,
        GetPlayerByIdUseCase getPlayerByIdUseCase)
    {
        _soloGameRepository = soloGameRepository;
        _energyRepository = energyRepository;
        _levelRepository = levelRepository;
        _worldRepository = worldRepository;
        _getQuestionsUseCase = getQuestionsUseCase;
        _getPlayerByIdUseCase = getPlayerByIdUseCase;
    }

    public async Task<SoloGame> ExecuteAsync(string uid, int levelId)
    {
        // 1. Obtener jugador por UID
        var player = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // 2. Verificar energía 
        var hasEnergy = await _energyRepository.HasEnergyAsync(player.Id);
        if (!hasEnergy)
        {
            throw new BusinessException("No tienes energía suficiente para jugar. Espera a que se regenere.");
        }

        // 3. Obtener configuración del nivel
        var level = await _levelRepository.GetByIdAsync(levelId);
        if (level == null)
        {
            throw new NotFoundException($"Nivel con ID {levelId} no encontrado");
        }

        // 4. Obtener configuración del mundo
        var allWorlds = await _worldRepository.GetAllWorldsAsync();
        var world = allWorlds.FirstOrDefault(w => w.Id == level.WorldId);
        if (world == null)
        {
            throw new NotFoundException($"Mundo con ID {level.WorldId} no encontrado");
        }

        // 5. Generar preguntas según la configuración del nivel
        var equationParams = new EquationParams
        {
            TermCount = level.TermsCount,
            VariableCount = level.VariablesCount,
            Operations = world.Operations,
            ExpectedResult = level.ResultType,
            OptionsCount = world.OptionsCount,
            OptionRangeMin = world.OptionRangeMin,
            OptionRangeMax = world.OptionRangeMax,
            NumberRangeMin = world.NumberRangeMin,
            NumberRangeMax = world.NumberRangeMax,
            TimePerEquation = world.TimePerEquation
        };

        var questions = await _getQuestionsUseCase.GetQuestions(equationParams, 10);

        // 6. Crear partida 
        var soloGame = new SoloGame
        {
            PlayerId = player.Id,
            PlayerUid = uid, 
            PlayerName = player.Name,
            LevelId = levelId,
            WorldId = level.WorldId,
            Questions = questions,
            TotalQuestions = 10,
            TimePerEquation = world.TimePerEquation,
            GameStartedAt = DateTime.UtcNow,
            CurrentQuestionStartedAt = DateTime.UtcNow,
            Status = SoloGameStatus.InProgress
        };

        // 7. Guardar partida
        await _soloGameRepository.AddAsync(soloGame);

        return soloGame;
    }
}
