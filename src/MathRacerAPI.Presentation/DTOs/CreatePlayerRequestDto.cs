namespace MathRacerAPI.Presentation.DTOs
{
    public class CreatePlayerRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
    }
}
