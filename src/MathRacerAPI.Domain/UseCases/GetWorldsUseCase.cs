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
        /// Ejecuta la lógica de obtención de mundos y progreso del jugador
        /// </summary>
        /// <param name="playerId">ID del jugador</param>
        /// <returns>Todos los mundos + ID del último mundo disponible</returns>
        /// <exception cref="ValidationException">Cuando el ID del jugador es inválido</exception>
        /// <exception cref="NotFoundException">Cuando el jugador no existe</exception>
        /// <exception cref="BusinessException">Cuando no hay mundos disponibles</exception>
        public async Task<PlayerWorlds> ExecuteAsync(int playerId)
        {
            // 1. Validar y obtener jugador (delega validación a GetPlayerByIdUseCase)
            var player = await _getPlayerByIdUseCase.ExecuteAsync(playerId);

            // 2. Obtener todos los mundos del juego
            var allWorlds = await _worldRepository.GetAllWorldsAsync();

            // 3. Obtener el WorldId del último nivel completado del jugador
            var lastAvailableWorldId = await _worldRepository.GetWorldIdByLevelIdAsync(player.LastLevelId);

            // 4. Validar que el mundo exista
            if (!allWorlds.Any(w => w.Id == lastAvailableWorldId))
            {
                throw new BusinessException(
                    $"El mundo con ID {lastAvailableWorldId} no existe en el sistema.");
            }

            // 5. Retornar modelo completo
            return new PlayerWorlds
            {
                Worlds = allWorlds,
                LastAvailableWorldId = lastAvailableWorldId
            };
        }
    }
}


