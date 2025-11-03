using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Linq;
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
    private readonly IProductRepository _productRepository;
    private readonly IWildcardRepository _wildcardRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;

    public StartSoloGameUseCase(
        ISoloGameRepository soloGameRepository,
        IEnergyRepository energyRepository,
        ILevelRepository levelRepository,
        IWorldRepository worldRepository,
        IProductRepository productRepository,
        IWildcardRepository wildcardRepository,
        GetQuestionsUseCase getQuestionsUseCase,
        GetPlayerByIdUseCase getPlayerByIdUseCase)
    {
        _soloGameRepository = soloGameRepository;
        _energyRepository = energyRepository;
        _levelRepository = levelRepository;
        _worldRepository = worldRepository;
        _productRepository = productRepository;
        _wildcardRepository = wildcardRepository;
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

        // 5. Obtener productos activos del jugador
        var playerProducts = await _productRepository.GetActiveProductsByPlayerIdAsync(player.Id);
        if (playerProducts.Count != 3)
        {
            throw new BusinessException("Error, debes tener 3 productos activos (auto, personaje, fondo)");
        }

        // 6. Obtener productos aleatorios para la máquina
        var machineProducts = await _productRepository.GetRandomProductsForMachineAsync();
        if (machineProducts.Count != 3)
        {
            throw new BusinessException("Error al cargar productos de la máquina");
        }

        // 7. Obtener wildcards disponibles del jugador
        var wildcards = await _wildcardRepository.GetPlayerWildcardsAsync(player.Id);

        // 8. Generar preguntas según la configuración del nivel
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

        var questions = await _getQuestionsUseCase.GetQuestions(equationParams, 15);

        // 9. Crear partida 
        var soloGame = new SoloGame
        {
            PlayerId = player.Id,
            PlayerUid = uid, 
            PlayerName = player.Name,
            LevelId = levelId,
            WorldId = level.WorldId,
            ResultType = level.ResultType,
            Questions = questions,
            TotalQuestions = 10,
            TimePerEquation = world.TimePerEquation,
            GameStartedAt = DateTime.UtcNow,
            Status = SoloGameStatus.InProgress,
            PlayerProducts = playerProducts,    
            MachineProducts = machineProducts,
            AvailableWildcards = wildcards,
            ReviewTimeSeconds = 3 
        };

        // 10. Guardar partida
        await _soloGameRepository.AddAsync(soloGame);

        return soloGame;
    }
}
