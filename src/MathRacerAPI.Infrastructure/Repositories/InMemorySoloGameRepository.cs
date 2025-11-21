using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories;

/// <summary>
/// Repositorio en memoria para partidas individuales
/// </summary>
public class InMemorySoloGameRepository : ISoloGameRepository
{
    private static readonly ConcurrentDictionary<int, SoloGame> _games = new();
    private static int _nextId = 1;

    public Task<SoloGame> AddAsync(SoloGame game)
    {
        game.Id = _nextId++;
        _games[game.Id] = game;
        return Task.FromResult(game);
    }

    public Task<SoloGame?> GetByIdAsync(int id)
    {
        _games.TryGetValue(id, out var game);
        return Task.FromResult(game);
    }

    public Task UpdateAsync(SoloGame game)
    {
        if (_games.ContainsKey(game.Id))
        {
            _games[game.Id] = game;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        _games.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
