using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para activar un wildcard en una partida individual
/// </summary>
public class UseWildcardUseCase
{
    private readonly ISoloGameRepository _soloGameRepository;
    private readonly IWildcardRepository _wildcardRepository;
    private readonly Random _random = new();

    public UseWildcardUseCase(
        ISoloGameRepository soloGameRepository,
        IWildcardRepository wildcardRepository)
    {
        _soloGameRepository = soloGameRepository;
        _wildcardRepository = wildcardRepository;
    }

    public async Task<WildcardUsageResult> ExecuteAsync(int gameId, int wildcardId, string requestingPlayerUid)
    {
        // 1. Obtener y validar la partida
        var game = await _soloGameRepository.GetByIdAsync(gameId);
        
        if (game == null)
        {
            throw new NotFoundException($"Partida con ID {gameId} no encontrada");
        }

        if (game.PlayerUid != requestingPlayerUid)
        {
            throw new BusinessException("No tienes permiso para usar comodines en esta partida");
        }

        // 2. Validar que el wildcard pueda ser usado
        if (game.Status != SoloGameStatus.InProgress)
        {
            throw new BusinessException("No puedes usar comodines cuando el juego no está en progreso");
        }

        if (game.UsedWildcardTypes.Contains(wildcardId))
        {
            throw new BusinessException("Ya usaste este comodín en esta partida");
        }

        if (game.CurrentQuestionIndex >= game.Questions.Count)
        {
            throw new BusinessException("No hay pregunta actual para aplicar el comodín");
        }

        // 3. Verificar que el jugador tenga el wildcard disponible en base de datos
        var hasWildcard = await _wildcardRepository.HasWildcardAvailableAsync(game.PlayerId, wildcardId);
        if (!hasWildcard)
        {
            throw new BusinessException("No tienes este comodín disponible");
        }

        // 4. Verificar que el wildcard esté cargado en la partida
        var wildcardInGame = game.AvailableWildcards.FirstOrDefault(w => w.WildcardId == wildcardId);
        if (wildcardInGame == null || wildcardInGame.Quantity <= 0)
        {
            throw new BusinessException("Este comodín no está disponible en esta partida");
        }

        // 5. Aplicar el efecto del wildcard según su tipo
        var result = new WildcardUsageResult
        {
            WildcardId = wildcardId,
            Success = true,
            GameId = gameId,
            Game = game
        };

        var wildcardType = (WildcardType)wildcardId;

        switch (wildcardType)
        {
            case WildcardType.RemoveWrongOption:
                ApplyRemoveWrongOption(game, result);
                break;

            case WildcardType.SkipQuestion:
                ApplySkipQuestion(game, result);
                break;

            case WildcardType.DoubleProgress:
                ApplyDoubleProgress(game, result);
                break;

            default:
                throw new BusinessException("Tipo de comodín no válido");
        }

        // 6. Consumir el wildcard de la base de datos
        var consumed = await _wildcardRepository.ConsumeWildcardAsync(game.PlayerId, wildcardId);
        if (!consumed)
        {
            throw new BusinessException("Error al consumir el comodín");
        }

        // 7. Actualizar la cantidad en la partida
        wildcardInGame.Quantity--;

        // 8. Marcar como usado en la partida
        game.UsedWildcardTypes.Add(wildcardId);

        // 9. Guardar cambios
        await _soloGameRepository.UpdateAsync(game);

        result.RemainingQuantity = wildcardInGame.Quantity;

        return result;
    }

    /// <summary>
    /// Elimina una opción incorrecta de las opciones disponibles
    /// </summary>
    private void ApplyRemoveWrongOption(SoloGame game, WildcardUsageResult result)
    {
        var currentQuestion = game.Questions[game.CurrentQuestionIndex];
        
        var wrongOptions = currentQuestion.Options
            .Where(o => o != currentQuestion.CorrectAnswer)
            .ToList();

        if (wrongOptions.Count == 0)
        {
            throw new BusinessException("No hay opciones incorrectas para eliminar");
        }

        // Eliminar una opción incorrecta aleatoria
        var optionToRemove = wrongOptions[_random.Next(wrongOptions.Count)];
        
        var modifiedOptions = currentQuestion.Options
            .Where(o => o != optionToRemove)
            .ToList();

        // Guardar las opciones modificadas en el juego
        game.ModifiedOptions = modifiedOptions;

        result.Message = "Se eliminó una opción incorrecta";
        result.ModifiedOptions = modifiedOptions;
    }

    /// <summary>
    /// Salta a la siguiente pregunta sin penalización
    /// </summary>
    private void ApplySkipQuestion(SoloGame game, WildcardUsageResult result)
    {
        // Avanzar al siguiente índice
        game.CurrentQuestionIndex++;

        // Verificar si hay más preguntas
        if (game.CurrentQuestionIndex >= game.Questions.Count)
        {
            throw new BusinessException("No hay más preguntas disponibles para saltar");
        }

        // Resetear el tiempo de la última respuesta para dar tiempo completo en la nueva pregunta
        game.LastAnswerTime = DateTime.UtcNow;

        // Limpiar opciones modificadas previas
        game.ModifiedOptions = null;

        result.Message = "Pregunta cambiada exitosamente";
        result.NewQuestionIndex = game.CurrentQuestionIndex;
    }

    /// <summary>
    /// Activa el efecto de doble progreso para la siguiente respuesta correcta
    /// </summary>
    private void ApplyDoubleProgress(SoloGame game, WildcardUsageResult result)
    {
        game.HasDoubleProgressActive = true;

        result.Message = "Doble progreso activado. La siguiente respuesta correcta valdrá doble.";
        result.DoubleProgressActive = true;
    }
}

