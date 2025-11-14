using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para cargar el siguiente lote de ecuaciones
/// </summary>
public class LoadNextBatchUseCase
{
    private readonly IInfiniteGameRepository _infiniteGameRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IWorldRepository _worldRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly Random _random = new();

    public LoadNextBatchUseCase(
        IInfiniteGameRepository infiniteGameRepository,
        ILevelRepository levelRepository,
        IWorldRepository worldRepository,
        GetQuestionsUseCase getQuestionsUseCase)
    {
        _infiniteGameRepository = infiniteGameRepository;
        _levelRepository = levelRepository;
        _worldRepository = worldRepository;
        _getQuestionsUseCase = getQuestionsUseCase;
    }

    public async Task<InfiniteGame> ExecuteAsync(int gameId)
    {
        // 1. Obtener partida
        var game = await _infiniteGameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            throw new NotFoundException($"Partida infinita con ID {gameId} no encontrada");
        }

        // 2. Validar estado del juego
        if (game.AbandonedAt != null)
        {
            throw new BusinessException("La partida ha sido abandonada");
        }

        // 3. Incrementar número de lote
        game.CurrentBatch++;

        // 4. Generar nuevo lote de 9 ecuaciones
        game.Questions = await GenerateQuestionsForBatch(game.CurrentBatch);
        game.CurrentQuestionIndex = 0;

        // 5. Actualizar partida
        await _infiniteGameRepository.UpdateAsync(game);

        return game;
    }

    /// <summary>
    /// Genera 9 ecuaciones según el número de lote actual
    /// </summary>
    private async Task<List<InfiniteQuestion>> GenerateQuestionsForBatch(int batchNumber)
    {
        // Calcular mundo basado en el lote (cada 3 lotes = 1 mundo)
        var worldId = (batchNumber / 3) + 1;
        
        // Determinar qué conjunto de niveles usar dentro del lote actual
        var difficultySteps = new[] { 1, 6, 11 }; // Niveles a usar

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
            else
            {
                // Si no hay más niveles/mundos, mantener la última configuración
                var lastParams = await GetLastAvailableParams();
                if (lastParams != null)
                {
                    var questions = await _getQuestionsUseCase.GetQuestions(lastParams, 3);
                    
                    // Convertir Question a InfiniteQuestion con el ExpectedResult
                    foreach (var question in questions)
                    {
                        var infiniteQuestion = InfiniteQuestion.FromQuestion(question, lastParams.ExpectedResult);
                        allQuestions.Add(infiniteQuestion);
                    }
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
        var allWorlds = await _worldRepository.GetAllWorldsAsync();
        var world = allWorlds.FirstOrDefault(w => w.Id == worldId);

        if (world == null)
        {
            return null; // No hay más mundos
        }

        var levels = await _levelRepository.GetAllByWorldIdAsync(world.Id);
        var level = levels.FirstOrDefault(l => l.Number == levelNumber);

        if (level == null)
        {
            return null; // No hay ese nivel en el mundo
        }

        // Generar ExpectedResult aleatorio (50-50 entre MAYOR y MENOR)
        string randomExpectedResult = _random.Next(0, 2) == 0 ? "MAYOR" : "MENOR";

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

    /// <summary>
    /// Obtiene los parámetros del último nivel disponible en la BD
    /// </summary>
    private async Task<EquationParams?> GetLastAvailableParams()
    {
        var allWorlds = await _worldRepository.GetAllWorldsAsync();
        var lastWorld = allWorlds.OrderByDescending(w => w.Id).FirstOrDefault();

        if (lastWorld == null) return null;

        var levels = await _levelRepository.GetAllByWorldIdAsync(lastWorld.Id);
        var lastLevel = levels.OrderByDescending(l => l.Number).FirstOrDefault();

        if (lastLevel == null) return null;

        // Generar ExpectedResult aleatorio (50-50 entre MAYOR y MENOR)
        string randomExpectedResult = _random.Next(0, 2) == 0 ? "MAYOR" : "MENOR";

        return new EquationParams
        {
            TermCount = lastLevel.TermsCount,
            VariableCount = lastLevel.VariablesCount,
            Operations = lastWorld.Operations,
            ExpectedResult = randomExpectedResult,
            OptionsCount = lastWorld.OptionsCount,
            OptionRangeMin = lastWorld.OptionRangeMin,
            OptionRangeMax = lastWorld.OptionRangeMax,
            NumberRangeMin = lastWorld.NumberRangeMin,
            NumberRangeMax = lastWorld.NumberRangeMax,
            TimePerEquation = lastWorld.TimePerEquation
        };
    }
}