using Google;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;

namespace MathRacerAPI.Infrastructure.Repositories;

public class InMemoryGameRepository : IGameRepository
{
    private static readonly Dictionary<int, Game> _games = new();
    private static readonly SemaphoreSlim _matchmakingLock = new SemaphoreSlim(1, 1);


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

    public Task<List<Game>> GetAllAsync()
    {
        return Task.FromResult(_games.Values.ToList());
    }

    public Task UpdateAsync(Game game)
    {
        _games[game.Id] = game;
        return Task.CompletedTask;
    }

    public Task<List<Game>> GetWaitingGames()
    {
        var waitingGames = _games.Values
            .Where(g => g.Status == GameStatus.WaitingForPlayers && g.Players.Count == 1)
            .OrderBy(g => g.CreatedAt)
            .ToList();

        return Task.FromResult(waitingGames); 
    }
     public Task<Game?> GetByIdWithPlayersAsync(int gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return Task.FromResult(game);
    }

}