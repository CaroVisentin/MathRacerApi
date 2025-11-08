namespace MathRacerAPI.Presentation.DTOs;

/// <summary>
/// DTO para representar el estado de energía de un jugador
/// </summary>
public class EnergyStatusDto
{
    /// <summary>
    /// Cantidad actual de energía
    /// </summary>
    public int CurrentAmount { get; set; }
    
    /// <summary>
    /// Cantidad máxima de energía
    /// </summary>
    public int MaxAmount { get; set; }
    
    /// <summary>
    /// Segundos restantes para la próxima recarga (null si está al máximo)
    /// </summary>
    public int? SecondsUntilNextRecharge { get; set; }
}