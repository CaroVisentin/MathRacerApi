namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Configuración de energía del juego
/// </summary>
public class EnergyConfiguration
{
    /// <summary>
    /// ID de la configuración
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Precio por unidad de energía
    /// </summary>
    public int Price { get; set; }
    
    /// <summary>
    /// Cantidad máxima de energía permitida
    /// </summary>
    public int MaxAmount { get; set; }
}