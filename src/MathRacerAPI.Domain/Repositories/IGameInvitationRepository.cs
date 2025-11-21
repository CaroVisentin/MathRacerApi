using MathRacerAPI.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories
{
    public interface IGameInvitationRepository
    {
        Task<GameInvitation> CreateAsync(GameInvitation invitation);
        Task<GameInvitation?> GetByIdAsync(int id);
        Task<List<GameInvitation>> GetPendingInvitationsForPlayerAsync(int playerId);
        Task UpdateStatusAsync(int invitationId, InvitationStatus status);
        Task DeleteAsync(int invitationId);
        Task<GameInvitation?> GetByGameIdAsync(int gameId);
    }
}