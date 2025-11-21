using System;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class GameInvitationEntity
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int InviterPlayerId { get; set; }
        public int InvitedPlayerId { get; set; }
        public int InvitationStatusId { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        
        // Navegación
        public PlayerEntity InviterPlayer { get; set; } = null!;
        public PlayerEntity InvitedPlayer { get; set; } = null!;
        public InvitationStatusEntity InvitationStatus { get; set; } = null!;
    }
}