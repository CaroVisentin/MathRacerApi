using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Infrastructure.Repositories;

public class InMemoryGameRepository : IGameRepository
{
    private static readonly Dictionary<int, Game> _games = new();

    public Task<Game> AddAsync(Game game)
    {
        _games[game.Id] = game;
        return Task.FromResult(game);
    }

    public Task<Game?> GetByIdAsync(int id)
    {
        _games.TryGetValue(id, out var game);
        return Task.FromResult(game);
    }

    public Task UpdateAsync(Game game)
    {
        _games[game.Id] = game;
        return Task.CompletedTask;
    }
}