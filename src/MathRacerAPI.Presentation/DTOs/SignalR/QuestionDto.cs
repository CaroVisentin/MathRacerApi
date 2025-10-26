using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Presentation.DTOs.SignalR;

/// <summary>
/// DTO para representar una pregunta en SignalR
/// </summary>
public class QuestionDto
{
    public int Id { get; set; }
    public string Equation { get; set; } = string.Empty;
    public List<int> Options { get; set; } = new();
    public int CorrectAnswer { get; set; }

    /// <summary>
    /// Convierte una Question a QuestionDto (incluyendo la respuesta correcta)
    /// </summary>
    public static QuestionDto FromQuestion(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Equation = question.Equation,
            Options = question.Options,
            CorrectAnswer = question.CorrectAnswer
        };
    }
}