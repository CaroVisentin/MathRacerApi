namespace MathRacerAPI.Presentation.DTOs.Infinite;

public class StartInfiniteGameResponseDto
{
    public int GameId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public List<InfiniteQuestionDto> Questions { get; set; } = new();
    public int TotalCorrectAnswers { get; set; }
    public int CurrentBatch { get; set; }
}