using System;

namespace MathRacerAPI.Presentation.DTOs
{
    public class GameInvitationDto
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string InviterPlayerName { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string ExpectedResult { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}