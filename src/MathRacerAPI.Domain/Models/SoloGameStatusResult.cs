namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Resultado con información de estado de una partida individual
/// </summary>
public class SoloGameStatusResult
{
    public SoloGame Game { get; set; } = new();
    public double RemainingTimeForQuestion { get; set; }
    public double ElapsedTime { get; set; }
}