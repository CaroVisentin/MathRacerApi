namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// Respuesta al enviar una respuesta - Solo feedback esencial
/// </summary>
public class SubmitSoloAnswerResponseDto
{
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    public int PlayerAnswer { get; set; }
    
    // Estado crítico del juego
    public string Status { get; set; } = string.Empty;
    public int LivesRemaining { get; set; }
    public int PlayerPosition { get; set; }
    public int TotalQuestions { get; set; }
    
    // Ganador (solo si terminó)
    public int? WinnerId { get; set; }
    public string? WinnerName { get; set; }
}