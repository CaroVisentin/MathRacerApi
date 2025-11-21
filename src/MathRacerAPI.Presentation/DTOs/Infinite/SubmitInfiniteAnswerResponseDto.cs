namespace MathRacerAPI.Presentation.DTOs.Infinite;

public class SubmitInfiniteAnswerResponseDto
{
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public bool NeedsNewBatch { get; set; }
}