using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener el estado actual de una partida individual
/// </summary>
public class GetSoloGameStatusUseCase
{
    private readonly ISoloGameRepository _soloGameRepository;

    public GetSoloGameStatusUseCase(ISoloGameRepository soloGameRepository)
    {
        _soloGameRepository = soloGameRepository;
    }

    public async Task<SoloGameStatusResult> ExecuteAsync(int gameId, string? requestingPlayerUid = null)
    {
        var game = await _soloGameRepository.GetByIdAsync(gameId);
        
        if (game == null)
        {
            throw new NotFoundException($"Partida con ID {gameId} no encontrada");
        }

        // Validar que el jugador que solicita es el dueño de la partida
        if (!string.IsNullOrEmpty(requestingPlayerUid) && game.PlayerUid != requestingPlayerUid)
        {
            throw new BusinessException("No tienes permiso para acceder a esta partida");
        }

        // Actualizar posición de la máquina si el juego está en progreso
        if (game.Status == SoloGameStatus.InProgress)
        {
            UpdateMachinePosition(game);
        }

        // CALCULAR TIEMPO RESTANTE (lógica movida del controller)
        var remainingTime = CalculateRemainingTime(game);

        // CALCULAR TIEMPO TRANSCURRIDO
        var elapsedTime = (DateTime.UtcNow - game.GameStartedAt).TotalSeconds;

        return new SoloGameStatusResult
        {
            Game = game,
            RemainingTimeForQuestion = remainingTime,
            ElapsedTime = elapsedTime
        };
    }

    /// <summary>
    /// Calcula el tiempo restante para la pregunta actual
    /// </summary>
    private double CalculateRemainingTime(SoloGame game)
    {
        if (!game.CurrentQuestionStartedAt.HasValue || game.Status != SoloGameStatus.InProgress)
        {
            return 0.0;
        }

        var elapsed = (DateTime.UtcNow - game.CurrentQuestionStartedAt.Value).TotalSeconds;
        return Math.Max(0, game.TimePerEquation - elapsed);
    }

    private void UpdateMachinePosition(SoloGame game)
    {
        var elapsedTime = (DateTime.UtcNow - game.GameStartedAt).TotalSeconds;
        var totalEstimatedTime = game.TotalEstimatedTime;
        
        var progress = elapsedTime / totalEstimatedTime;
        game.MachinePosition = (int)(progress * game.TotalQuestions);
        
        if (game.MachinePosition > game.TotalQuestions)
        {
            game.MachinePosition = game.TotalQuestions;
        }
    }
}
