namespace MathRacerAPI.Presentation.DTOs
{
    public class SendGameInvitationRequestDto
    {
        public int InvitedFriendId { get; set; }
        public string Difficulty { get; set; } = "facil";
        public string ExpectedResult { get; set; } = "MAYOR";
    }
}