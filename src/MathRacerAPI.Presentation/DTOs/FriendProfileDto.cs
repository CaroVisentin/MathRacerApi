namespace MathRacerAPI.Presentation.DTOs
{
    public class FriendProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
        public int Points { get; set; }
        public ProductDto? Character { get; set; }
    }
}
