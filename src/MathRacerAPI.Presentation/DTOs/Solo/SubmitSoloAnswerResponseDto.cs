using System;

namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// Respuesta al enviar una respuesta en una partida individual 
/// </summary>
public class SubmitSoloAnswerResponseDto
{
    // Feedback de la respuesta
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    public int PlayerAnswer { get; set; }
    
    // Estado del juego
    public string Status { get; set; } = string.Empty;
    public int LivesRemaining { get; set; }
    public int PlayerPosition { get; set; }
    public int MachinePosition { get; set; }
    public int CorrectAnswers { get; set; }
    
    // Control de tiempo
    public int WaitTimeSeconds { get; set; }
    public DateTime AnsweredAt { get; set; }
    
    public int CurrentQuestionIndex { get; set; }
}