using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    /// <summary>
    /// Caso de uso para obtener un jugador por su ID
    /// </summary>
    public class GetPlayerByIdUseCase
    {
        private readonly IPlayerRepository _playerRepository;

        public GetPlayerByIdUseCase(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        /// <summary>
        /// Ejecuta la lógica de obtención de un jugador
        /// </summary>
        /// <param name="playerId">ID del jugador</param>
        /// <returns>Jugador encontrado</returns>
        /// <exception cref="ValidationException">Cuando el ID es inválido</exception>
        /// <exception cref="NotFoundException">Cuando el jugador no existe</exception>
        public async Task<Player> ExecuteAsync(int playerId)
        {
            // Validación del ID
            if (playerId <= 0)
            {
                throw new ValidationException("El ID del jugador debe ser mayor a 0.");
            }

            // Obtener jugador del repositorio
            var player = await _playerRepository.GetByIdAsync(playerId);

            // Lanzar excepción si no existe
            if (player == null)
            {
                throw new NotFoundException("Jugador", playerId);
            }

            return player;
        }
    }
}
