using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Repositories;

public interface IGameRepository
{
    Task<Game> AddAsync(Game game);
    Task<Game?> GetByIdAsync(int id);
    Task<List<Game>> GetAllAsync();
    Task UpdateAsync(Game game);
    Task DeleteAsync(int gameId);
    Task<List<Game>> GetWaitingGames();
}