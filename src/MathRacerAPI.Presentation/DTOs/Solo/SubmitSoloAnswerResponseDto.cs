namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// Respuesta al enviar una respuesta - Contiene TODO lo necesario para continuar
/// </summary>
public class SubmitSoloAnswerResponseDto
{
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    public int PlayerAnswer { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public int LivesRemaining { get; set; }
    public int PlayerPosition { get; set; }
    public int MachinePosition { get; set; } 
    public int CorrectAnswers { get; set; }  
    
    public QuestionDto? NextQuestion { get; set; } 
    public int CurrentQuestionIndex { get; set; }  
    
    public int? WinnerId { get; set; }
    public string? WinnerName { get; set; }
}