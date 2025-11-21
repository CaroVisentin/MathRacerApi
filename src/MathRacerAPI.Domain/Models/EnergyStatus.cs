namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Modelo de dominio que representa el estado actual de energía de un jugador
/// </summary>
public class EnergyStatus
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
    
    /// <summary>
    /// Fecha y hora de la última recarga completa calculada
    /// </summary>
    public DateTime LastCalculatedRecharge { get; set; }
}