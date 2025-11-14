using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar partidas en modo infinito
/// </summary>
public interface IInfiniteGameRepository
{
    Task<InfiniteGame> AddAsync(InfiniteGame game);
    Task<InfiniteGame?> GetByIdAsync(int id);
    Task UpdateAsync(InfiniteGame game);
    Task DeleteAsync(int id);
}