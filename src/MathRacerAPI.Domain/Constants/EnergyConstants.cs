namespace MathRacerAPI.Domain.Constants;

/// <summary>
/// Constantes relacionadas con el sistema de energía del juego
/// </summary>
public static class EnergyConstants
{
    /// <summary>
    /// Cantidad máxima de energía que puede tener un jugador
    /// </summary>
    public const int MAX_ENERGY = 3;
    
    /// <summary>
    /// Tiempo en segundos necesario para recargar 1 punto de energía (15 minutos)
    /// </summary>
    public const int SECONDS_PER_RECHARGE = 900;

}