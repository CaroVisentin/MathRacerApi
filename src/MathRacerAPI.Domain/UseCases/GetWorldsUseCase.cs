using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    /// <summary>
    /// Caso de uso para obtener todos los mundos y el progreso del jugador
    /// </summary>
    public class GetWorldsUseCase
    {
        private readonly IWorldRepository _worldRepository;
        private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;

        public GetWorldsUseCase(
            IWorldRepository worldRepository,
            GetPlayerByIdUseCase getPlayerByIdUseCase)
        {
            _worldRepository = worldRepository;
            _getPlayerByIdUseCase = getPlayerByIdUseCase;
        }
      
        /// <summary>
        /// Ejecuta la lógica de obtención de mundos y progreso del jugador por UID
        /// </summary>
        /// <param name="uid">UID de Firebase del jugador</param>
        /// <returns>Todos los mundos + ID del último mundo disponible</returns>
        public async Task<PlayerWorlds> ExecuteByUidAsync(string uid)
        {
            var player = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);
            return await GetWorldsForPlayer(player);
        }

        /// <summary>
        /// Lógica común para obtener mundos de un jugador
        /// </summary>
        private async Task<PlayerWorlds> GetWorldsForPlayer(PlayerProfile player)
        {
            // 1. Obtener todos los mundos del juego
            var allWorlds = await _worldRepository.GetAllWorldsAsync();

            // 2. Obtener el WorldId del último nivel completado del jugador
            var lastAvailableWorldId = await _worldRepository.GetWorldIdByLevelIdAsync(player.LastLevelId);

            // 3. Validar que el mundo exista
            if (!allWorlds.Any(w => w.Id == lastAvailableWorldId))
            {
                throw new BusinessException(
                    $"El mundo con ID {lastAvailableWorldId} no existe en el sistema.");
            }

            // 4. Retornar modelo completo
            return new PlayerWorlds
            {
                Worlds = allWorlds,
                LastAvailableWorldId = lastAvailableWorldId
            };
        }
    }
}


