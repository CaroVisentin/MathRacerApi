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

    public async Task<SoloGame> ExecuteAsync(int gameId, string? requestingPlayerUid = null)
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

        return game;
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
