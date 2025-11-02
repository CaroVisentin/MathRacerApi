using MathRacerAPI.Domain.Models;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar partidas individuales
/// </summary>
public interface ISoloGameRepository
{
    Task<SoloGame> AddAsync(SoloGame game);
    Task<SoloGame?> GetByIdAsync(int id);
    Task UpdateAsync(SoloGame game);
    Task DeleteAsync(int id);
}
