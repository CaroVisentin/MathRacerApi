using System;

namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Resultado con el estado completo de una partida individual
/// </summary>
public class SoloGameStatusResult
{
    public SoloGame Game { get; set; } = new();
    public double ElapsedTime { get; set; }
    
}