using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Infrastructure.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de partidas infinitas
/// </summary>
public class InMemoryInfiniteGameRepository : IInfiniteGameRepository
{
    private readonly Dictionary<int, InfiniteGame> _games = new();
    private int _nextId = 1;
    private readonly object _lock = new();

    public Task<InfiniteGame> AddAsync(InfiniteGame game)
    {
        lock (_lock)
        {
            game.Id = _nextId++;
            _games[game.Id] = game;
            return Task.FromResult(game);
        }
    }

    public Task<InfiniteGame?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            _games.TryGetValue(id, out var game);
            return Task.FromResult(game);
        }
    }

    public Task UpdateAsync(InfiniteGame game)
    {
        lock (_lock)
        {
            if (_games.ContainsKey(game.Id))
            {
                _games[game.Id] = game;
            }
            return Task.CompletedTask;
        }
    }

    public Task DeleteAsync(int id)
    {
        lock (_lock)
        {
            _games.Remove(id);
            return Task.CompletedTask;
        }
    }
}