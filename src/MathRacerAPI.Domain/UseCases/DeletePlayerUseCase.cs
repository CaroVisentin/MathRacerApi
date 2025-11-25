using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Repositories;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    /// <summary>
    /// Caso de uso para realizar la baja lógica de un jugador
    /// </summary>
    public class DeletePlayerUseCase
    {
        private readonly IPlayerRepository _playerRepository;

        public DeletePlayerUseCase(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        /// <summary>
        /// Ejecuta la lógica de eliminación lógica del jugador
        /// </summary>
        /// <param name="uid">UID del jugador a eliminar</param>
        /// <exception cref="ValidationException">Cuando el UID es inválido</exception>
        public async Task ExecuteAsync(string uid)
        {
            // Validación del UID
            if (string.IsNullOrWhiteSpace(uid))
                throw new ValidationException("El UID es requerido");

            // Verificar que el jugador existe antes de eliminarlo
            var player = await _playerRepository.GetByUidAsync(uid);
            if (player == null)
                throw new NotFoundException("No se encontró un jugador con el UID proporcionado.");

            // Realizar la baja lógica
            await _playerRepository.DeleteAsync(uid);
        }
    }
}