using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Repositories;

public interface IGameRepository
{
    Task<Game> AddAsync(Game game);
    Task<Game?> GetByIdAsync(int id);
    Task UpdateAsync(Game game);
}