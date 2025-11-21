namespace MathRacerAPI.Presentation.DTOs.Infinite;

public class InfiniteQuestionDto
{
    public int QuestionId { get; set; }
    public string Equation { get; set; } = string.Empty;
    public List<int> Options { get; set; } = new();
    public int CorrectAnswer { get; set; }
    public string ExpectedResult { get; set; } = string.Empty;
}