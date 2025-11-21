namespace MathRacerAPI.Presentation.DTOs;

public class PlayerRankingDto
{
    public int Position { get; set; }
    public int PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Points { get; set; }
}
