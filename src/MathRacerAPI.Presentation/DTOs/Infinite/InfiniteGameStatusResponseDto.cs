namespace MathRacerAPI.Presentation.DTOs.Infinite;

public class InfiniteGameStatusResponseDto
{
    public int GameId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int TotalCorrectAnswers { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int CurrentBatch { get; set; }
    public bool IsActive { get; set; }
    public DateTime GameStartedAt { get; set; }
    public DateTime? AbandonedAt { get; set; }
}