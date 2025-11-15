using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener el estado actual de una partida infinita
/// </summary>
public class GetInfiniteGameStatusUseCase
{
    private readonly IInfiniteGameRepository _infiniteGameRepository;

    public GetInfiniteGameStatusUseCase(IInfiniteGameRepository infiniteGameRepository)
    {
        _infiniteGameRepository = infiniteGameRepository;
    }

    public async Task<InfiniteGame> ExecuteAsync(int gameId)
    {
        var game = await _infiniteGameRepository.GetByIdAsync(gameId);
        
        if (game == null)
        {
            throw new NotFoundException($"Partida infinita con ID {gameId} no encontrada");
        }

        return game;
    }
}