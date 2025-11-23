using System;

namespace MathRacerAPI.Domain.Models
{
    public class GameInvitation
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int InviterPlayerId { get; set; }
        public string InviterPlayerName { get; set; } = string.Empty;
        public int InvitedPlayerId { get; set; }
        public string InvitedPlayerName { get; set; } = string.Empty;
        public InvitationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string ExpectedResult { get; set; } = string.Empty;
    }

    public enum InvitationStatus
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3
    }
}