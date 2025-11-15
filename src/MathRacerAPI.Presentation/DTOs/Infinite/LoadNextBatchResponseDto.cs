namespace MathRacerAPI.Presentation.DTOs.Infinite;

public class LoadNextBatchResponseDto
{
    public int GameId { get; set; }
    public List<InfiniteQuestionDto> Questions { get; set; } = new();
    public int CurrentBatch { get; set; }
    public int TotalCorrectAnswers { get; set; }
}