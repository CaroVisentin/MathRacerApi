namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Resultado de procesar una respuesta en modo infinito
/// </summary>
public class InfiniteAnswerResult
{
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public bool NeedsNewBatch { get; set; } // Indica si se necesita cargar nuevo lote
}