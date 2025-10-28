using MathRacerAPI.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories
{
    /// <summary>
    /// Repositorio para operaciones con mundos
    /// </summary>
    public interface IWorldRepository
    {
        /// <summary>
        /// Obtiene todos los mundos del juego (sin niveles)
        /// </summary>
        /// <returns>Lista de todos los mundos</returns>
        Task<List<World>> GetAllWorldsAsync();

        /// <summary>
        /// Obtiene el ID del mundo al que pertenece un nivel específico
        /// </summary>
        /// <param name="levelId">ID del nivel</param>
        /// <returns>ID del mundo, o 1 si el nivel no existe</returns>
        Task<int> GetWorldIdByLevelIdAsync(int levelId);
    }
}
