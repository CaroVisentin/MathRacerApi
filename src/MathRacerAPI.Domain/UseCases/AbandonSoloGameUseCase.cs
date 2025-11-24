using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para abandonar una partida en modo individual
/// Deduce energía del jugador al abandonar
/// </summary>
public class AbandonSoloGameUseCase
{
    private readonly ISoloGameRepository _soloGameRepository;
    private readonly IEnergyRepository _energyRepository;

    public AbandonSoloGameUseCase(
        ISoloGameRepository soloGameRepository,
        IEnergyRepository energyRepository)
    {
        _soloGameRepository = soloGameRepository;
        _energyRepository = energyRepository;
    }

    public async Task<SoloGame> ExecuteAsync(int gameId, string requestingPlayerUid)
    {
        // 1. Obtener partida
        var game = await _soloGameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            throw new NotFoundException($"Partida con ID {gameId} no encontrada");
        }

        // 2. Validar que el jugador es el dueño de la partida
        if (game.PlayerUid != requestingPlayerUid)
        {
            throw new BusinessException("No tienes permiso para abandonar esta partida");
        }

        // 3. Validar que la partida está en progreso
        if (game.Status != SoloGameStatus.InProgress)
        {
            throw new BusinessException("La partida ya finalizó, no se puede abandonar");
        }

        // 4. Marcar como perdida y deducir energía
        game.Status = SoloGameStatus.PlayerLost;
        game.GameFinishedAt = DateTime.UtcNow;
        game.LivesRemaining = 0; // Marcar como sin vidas

        // 5. Consumir energía del jugador
        await _energyRepository.ConsumeEnergyAsync(game.PlayerId);

        // 6. Actualizar partida
        await _soloGameRepository.UpdateAsync(game);

        return game;
    }
}