using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    /// <summary>
    /// Caso de uso para obtener todos los niveles de un mundo y el progreso del jugador
    /// </summary>
    public class GetWorldLevelsUseCase
    {
        private readonly ILevelRepository _levelRepository;
        private readonly IWorldRepository _worldRepository;
        private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;

        public GetWorldLevelsUseCase(
            ILevelRepository levelRepository,
            IWorldRepository worldRepository,
            GetPlayerByIdUseCase getPlayerByIdUseCase)
        {
            _levelRepository = levelRepository;
            _worldRepository = worldRepository;
            _getPlayerByIdUseCase = getPlayerByIdUseCase;
        }

        /// <summary>
        /// Ejecuta la lógica para obtener niveles de un mundo y progreso del jugador
        /// </summary>
        /// <param name="playerId">ID del jugador</param>
        /// <param name="worldId">ID del mundo</param>
        /// <returns>Información completa de niveles y progreso</returns>
        /// <exception cref="ValidationException">Cuando los IDs son inválidos</exception>
        /// <exception cref="NotFoundException">Cuando el jugador o mundo no existe</exception>
        /// <exception cref="BusinessException">Cuando no hay niveles disponibles</exception>
        public async Task<PlayerWorldLevels> ExecuteAsync(int playerId, int worldId)
        {
            // 1. Validar worldId
            if (worldId <= 0)
                throw new ValidationException("El ID del mundo debe ser mayor a 0");

            // 2. Obtener y validar jugador (delega validación de playerId)
            var player = await _getPlayerByIdUseCase.ExecuteAsync(playerId);

            // 3. Obtener todos los mundos para validar que el worldId existe
            var allWorlds = await _worldRepository.GetAllWorldsAsync();
            var world = allWorlds.FirstOrDefault(w => w.Id == worldId);

            if (world == null)
                throw new NotFoundException($"Mundo con ID {worldId} no fue encontrado");

            // 4. Obtener todos los niveles del mundo
            var levels = await _levelRepository.GetAllByWorldIdAsync(worldId);

            if (!levels.Any())
                throw new BusinessException($"El mundo '{world.Name}' no tiene niveles configurados");

            // 5. Retornar modelo completo
            return new PlayerWorldLevels
            {
                WorldName = world.Name,
                Levels = levels,
                LastCompletedLevelId = player.LastLevelId,
            };
        }
    }
}
