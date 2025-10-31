using MathRacerAPI.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories
{
    /// <summary>
    /// Repositorio para operaciones con niveles
    /// </summary>
    public interface ILevelRepository
    {
        /// <summary>
        /// Obtiene un nivel por su ID
        /// </summary>
        /// <param name="id">ID del nivel</param>
        /// <returns>Nivel encontrado o null</returns>
        Task<Level?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todos los niveles de un mundo específico
        /// </summary>
        /// <param name="worldId">ID del mundo</param>
        /// <returns>Lista de niveles del mundo ordenados por número</returns>
        Task<List<Level>> GetAllByWorldIdAsync(int worldId);
    }
}
