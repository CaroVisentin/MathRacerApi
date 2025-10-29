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

        /// <summary>
        /// Ejecuta la lógica de obtención de un jugador por su email
        /// </summary>
        /// <param name="email">Email del jugador</param>
        /// <returns>Jugador encontrado o null</returns>
        public async Task<PlayerProfile?> ExecuteByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("El email es requerido");

            var playerProfile = await _playerRepository.GetByEmailAsync(email);
            return playerProfile;
        }
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
        public async Task<PlayerProfile> ExecuteAsync(int playerId)
        {
            // Validación del ID
            if (playerId <= 0)
                throw new ValidationException("El ID del jugador debe ser mayor a 0");

            // Obtener jugador del repositorio
            var playerProfile = await _playerRepository.GetByIdAsync(playerId);

            // Lanzar excepción si no existe
            if (playerProfile == null)
                throw new NotFoundException($"Jugador con ID {playerId} no fue encontrado");

            return playerProfile;
        }

        /// <summary>
        /// Ejecuta la lógica de obtención de un jugador por su UID
        /// </summary>
        /// <param name="uid">UID del jugador</param>
        /// <returns>Jugador encontrado</returns>
        /// <exception cref="ValidationException">Cuando el UID es inválido</exception>
        /// <exception cref="NotFoundException">Cuando el jugador no existe</exception>
        public async Task<PlayerProfile> ExecuteByUidAsync(string uid)
        {
            // Validación del UID
            if (string.IsNullOrWhiteSpace(uid))
                throw new ValidationException("El UID es requerido");

            // Obtener jugador del repositorio
            var playerProfile = await _playerRepository.GetByUidAsync(uid);

            // Lanzar excepción si no existe
            if (playerProfile == null)
                throw new NotFoundException("No se encontró un jugador con el UID proporcionado");

            return playerProfile;
        }
    }
}
