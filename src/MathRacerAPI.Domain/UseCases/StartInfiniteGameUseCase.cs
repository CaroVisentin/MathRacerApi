using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para iniciar una partida en modo infinito
/// </summary>
public class StartInfiniteGameUseCase
{
    private readonly IInfiniteGameRepository _infiniteGameRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IWorldRepository _worldRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;
    private readonly Random _random = new();

    public StartInfiniteGameUseCase(
        IInfiniteGameRepository infiniteGameRepository,
        ILevelRepository levelRepository,
        IWorldRepository worldRepository,
        GetQuestionsUseCase getQuestionsUseCase,
        GetPlayerByIdUseCase getPlayerByIdUseCase)
    {
        _infiniteGameRepository = infiniteGameRepository;
        _levelRepository = levelRepository;
        _worldRepository = worldRepository;
        _getQuestionsUseCase = getQuestionsUseCase;
        _getPlayerByIdUseCase = getPlayerByIdUseCase;
    }

    public async Task<InfiniteGame> ExecuteAsync(string uid)
    {
        // 1. Obtener jugador por UID
        var player = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // 2. Generar primer lote de 9 ecuaciones
        var questions = await GenerateQuestionsForBatch(0);

        // 3. Crear partida infinita
        var infiniteGame = new InfiniteGame
        {
            PlayerId = player.Id,
            PlayerUid = uid,
            PlayerName = player.Name,
            Questions = questions,
            CurrentBatch = 0,
            CurrentWorldId = 1,
            CurrentDifficultyStep = 0,
            GameStartedAt = DateTime.UtcNow,
            AbandonedAt = null 
        };

        // 4. Guardar partida
        await _infiniteGameRepository.AddAsync(infiniteGame);

        return infiniteGame;
    }

    /// <summary>
    /// Genera 9 ecuaciones según el número de lote actual
    /// </summary>
    private async Task<List<InfiniteQuestion>> GenerateQuestionsForBatch(int batchNumber)
    {
        // Calcular mundo y pasos de dificultad basados en el lote
        var worldId = (batchNumber / 3) + 1; // Cada 3 lotes cambia de mundo
        var difficultySteps = new[] { 1, 6, 11 }; // Niveles a usar dentro de cada mundo

        var allQuestions = new List<InfiniteQuestion>();

        // Generar 3 ecuaciones para cada nivel de dificultad
        for (int i = 0; i < 3; i++)
        {
            var levelNumber = difficultySteps[i];
            var equationParams = await GetEquationParamsForLevel(worldId, levelNumber);

            if (equationParams != null)
            {
                var questions = await _getQuestionsUseCase.GetQuestions(equationParams, 3);
                
                // Convertir Question a InfiniteQuestion con el ExpectedResult
                foreach (var question in questions)
                {
                    var infiniteQuestion = InfiniteQuestion.FromQuestion(question, equationParams.ExpectedResult);
                    allQuestions.Add(infiniteQuestion);
                }
            }
        }

        return allQuestions;
    }

    /// <summary>
    /// Obtiene los parámetros de ecuación para un mundo y nivel específico
    /// </summary>
    private async Task<EquationParams?> GetEquationParamsForLevel(int worldId, int levelNumber)
    {
        // Obtener todos los mundos
        var allWorlds = await _worldRepository.GetAllWorldsAsync();
        var world = allWorlds.FirstOrDefault(w => w.Id == worldId);

        if (world == null)
        {
            // Si no existe el mundo, usar el último disponible
            world = allWorlds.OrderByDescending(w => w.Id).FirstOrDefault();
            if (world == null) return null;
        }

        // Obtener nivel específico del mundo
        var levels = await _levelRepository.GetAllByWorldIdAsync(world.Id);
        var level = levels.FirstOrDefault(l => l.Number == levelNumber);

        if (level == null)
        {
            // Si no existe el nivel, usar el último del mundo
            level = levels.OrderByDescending(l => l.Number).FirstOrDefault();
            if (level == null) return null;
        }

        // Generar ExpectedResult aleatorio (50-50 entre MAYOR y MENOR)
        string randomExpectedResult = _random.Next(0, 2) == 0 ? "MAYOR" : "MENOR";

        // Construir parámetros de ecuación
        return new EquationParams
        {
            TermCount = level.TermsCount,
            VariableCount = level.VariablesCount,
            Operations = world.Operations,
            ExpectedResult = randomExpectedResult,
            OptionsCount = world.OptionsCount,
            OptionRangeMin = world.OptionRangeMin,
            OptionRangeMax = world.OptionRangeMax,
            NumberRangeMin = world.NumberRangeMin,
            NumberRangeMax = world.NumberRangeMax,
            TimePerEquation = world.TimePerEquation
        };
    }
}