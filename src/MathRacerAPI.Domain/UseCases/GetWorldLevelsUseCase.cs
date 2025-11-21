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
        /// Ejecuta la lógica para obtener niveles de un mundo y progreso del jugador por UID
        /// </summary>
        /// <param name="uid">UID de Firebase del jugador</param>
        /// <param name="worldId">ID del mundo</param>
        /// <returns>Información completa de niveles y progreso</returns>
        /// <exception cref="ValidationException">Cuando los IDs son inválidos</exception>
        /// <exception cref="NotFoundException">Cuando el jugador o mundo no existe</exception>
        /// <exception cref="BusinessException">Cuando no hay niveles disponibles o el mundo no está desbloqueado</exception>
        public async Task<PlayerWorldLevels> ExecuteByUidAsync(string uid, int worldId)
        {
            var player = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);
            return await GetLevelsForPlayer(player, worldId);
        }

        /// <summary>
        /// Lógica común para obtener niveles de un mundo y progreso del jugador
        /// </summary>
        private async Task<PlayerWorldLevels> GetLevelsForPlayer(PlayerProfile player, int worldId)
        {
            // 1. Validar worldId
            if (worldId <= 0)
                throw new ValidationException("El ID del mundo debe ser mayor a 0");

            // 2. Obtener el LastLevelId del jugador (manejar nullable y valor 0)
            // Si es null o 0, significa que no ha completado ningún nivel
            int lastLevelId = player.LastLevelId ?? 0;

            // 3. Obtener el ID del mundo actual del jugador según su último nivel completado
            var playerCurrentWorldId = await _worldRepository.GetWorldIdByLevelIdAsync(lastLevelId);

            // 4. Validar que el jugador tenga acceso al mundo solicitado
            if (worldId > playerCurrentWorldId)
            {
                throw new BusinessException(
                    $"No tienes acceso al mundo {worldId}. Completa los niveles del mundo {playerCurrentWorldId} para desbloquearlo.");
            }

            // 5. Obtener todos los mundos para validar que el worldId existe
            var allWorlds = await _worldRepository.GetAllWorldsAsync();
            var world = allWorlds.FirstOrDefault(w => w.Id == worldId);

            if (world == null)
                throw new NotFoundException($"Mundo con ID {worldId} no fue encontrado");

            // 6. Obtener todos los niveles del mundo
            var levels = await _levelRepository.GetAllByWorldIdAsync(worldId);

            if (!levels.Any())
                throw new BusinessException($"El mundo '{world.Name}' no tiene niveles configurados");

            // 7. Retornar modelo completo
            return new PlayerWorldLevels
            {
                WorldName = world.Name,
                Levels = levels,
                LastCompletedLevelId = lastLevelId, // Usar el valor procesado (0 si no completó ninguno)
            };
        }
    }
}
