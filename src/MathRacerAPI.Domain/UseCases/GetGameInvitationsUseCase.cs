using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    /// <summary>
    /// Caso de uso para obtener las invitaciones pendientes de un jugador (buzón)
    /// </summary>
    public class GetGameInvitationsUseCase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameInvitationRepository _invitationRepository;

        public GetGameInvitationsUseCase(
            IPlayerRepository playerRepository,
            IGameInvitationRepository invitationRepository)
        {
            _playerRepository = playerRepository;
            _invitationRepository = invitationRepository;
        }

        public async Task<List<GameInvitation>> ExecuteAsync(string firebaseUid)
        {
            var playerProfile = await _playerRepository.GetByUidAsync(firebaseUid);
            if (playerProfile == null)
                throw new NotFoundException("Perfil de jugador no encontrado");

            return await _invitationRepository.GetPendingInvitationsForPlayerAsync(playerProfile.Id);
        }
    }
}