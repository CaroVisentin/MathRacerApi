using System.Collections.Generic;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class InvitationStatusEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<GameInvitationEntity> GameInvitations { get; set; } = new List<GameInvitationEntity>();
    }
}