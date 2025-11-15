using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para abandonar una partida en modo infinito
/// </summary>
public class AbandonInfiniteGameUseCase
{
    private readonly IInfiniteGameRepository _infiniteGameRepository;

    public AbandonInfiniteGameUseCase(IInfiniteGameRepository infiniteGameRepository)
    {
        _infiniteGameRepository = infiniteGameRepository;
    }

    public async Task<InfiniteGame> ExecuteAsync(int gameId)
    {
        // 1. Obtener partida
        var game = await _infiniteGameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            throw new NotFoundException($"Partida infinita con ID {gameId} no encontrada");
        }

        // 2. Validar que no esté ya abandonada
        if (!game.IsActive)
        {
            throw new BusinessException("La partida ya ha sido abandonada");
        }

        // 3. Marcar como abandonada
        game.AbandonedAt = DateTime.UtcNow;

        // 4. Actualizar en repositorio
        await _infiniteGameRepository.UpdateAsync(game);

        return game;
    }
}