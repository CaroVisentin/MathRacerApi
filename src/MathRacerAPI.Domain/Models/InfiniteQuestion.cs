namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa una pregunta en el modo infinito con información adicional
/// </summary>
public class InfiniteQuestion
{
    public int Id { get; set; }
    public string Equation { get; set; } = string.Empty;
    public List<int> Options { get; set; } = new();
    public int CorrectAnswer { get; set; }
    public string ExpectedResult { get; set; } = string.Empty;

    /// <summary>
    /// Crea un InfiniteQuestion a partir de un Question y un ExpectedResult
    /// </summary>
    public static InfiniteQuestion FromQuestion(Question question, string expectedResult)
    {
        return new InfiniteQuestion
        {
            Id = question.Id,
            Equation = question.Equation,
            Options = new List<int>(question.Options),
            CorrectAnswer = question.CorrectAnswer,
            ExpectedResult = expectedResult
        };
    }
}