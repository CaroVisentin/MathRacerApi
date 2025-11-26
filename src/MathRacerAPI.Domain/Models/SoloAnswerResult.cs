using System;

namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Resultado de procesar una respuesta en modo individual
/// </summary>
public class SoloAnswerResult
{
    public SoloGame Game { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    public int PlayerAnswer { get; set; }
    public bool ShouldOpenWorldCompletionChest { get; set; }
    public int ProgressIncrement { get; set; } = 1;
    public int CoinsEarned { get; set; }
    public int RemainingCoins { get; set; }
}
