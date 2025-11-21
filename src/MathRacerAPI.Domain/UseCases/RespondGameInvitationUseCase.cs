using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    /// <summary>
    /// Caso de uso para aceptar o rechazar una invitación de partida
    /// </summary>
    public class RespondGameInvitationUseCase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameInvitationRepository _invitationRepository;
        private readonly IGameRepository _gameRepository;

        public RespondGameInvitationUseCase(
            IPlayerRepository playerRepository,
            IGameInvitationRepository invitationRepository,
            IGameRepository gameRepository)
        {
            _playerRepository = playerRepository;
            _invitationRepository = invitationRepository;
            _gameRepository = gameRepository;
        }

        public async Task<(bool accepted, int? gameId)> ExecuteAsync(
            string firebaseUid,
            int invitationId,
            bool accept)
        {
            // Verificar que el jugador existe
            var playerProfile = await _playerRepository.GetByUidAsync(firebaseUid);
            if (playerProfile == null)
                throw new NotFoundException("Perfil de jugador no encontrado");

            // Obtener invitación
            var invitation = await _invitationRepository.GetByIdAsync(invitationId);
            if (invitation == null)
                throw new NotFoundException("Invitación no encontrada");

            // Verificar que el jugador es el invitado
            if (invitation.InvitedPlayerId != playerProfile.Id)
                throw new ValidationException("No tienes permiso para responder esta invitación");

            // Verificar que la invitación está pendiente
            if (invitation.Status != InvitationStatus.Pending)
                throw new ValidationException("Esta invitación ya fue respondida");

            if (accept)
            {
                // Actualizar estado de invitación
                await _invitationRepository.UpdateStatusAsync(invitationId, InvitationStatus.Accepted);
                return (true, invitation.GameId);
            }
            else
            {
                // Rechazar invitación
                await _invitationRepository.UpdateStatusAsync(invitationId, InvitationStatus.Rejected);
                
                // Eliminar partida si fue rechazada
                var game = await _gameRepository.GetByIdAsync(invitation.GameId);
                return (false, null);
            }
        }
    }
}