namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Tipos de power-ups disponibles en el juego
/// </summary>
public enum PowerUpType
{
    /// <summary>
    /// La siguiente respuesta correcta vale doble (2 puntos)
    /// </summary>
    DoublePoints = 1,
    
    /// <summary>
    /// Mezclar las opciones del rival
    /// </summary>
    ShuffleRival = 2
}

/// <summary>
/// Representa un power-up que puede usar un jugador
/// </summary>
public class PowerUp
{
    public int Id { get; set; }
    public PowerUpType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Efecto activo de un power-up en el juego
/// </summary>
public class ActiveEffect
{
    public int Id { get; set; }
    public PowerUpType Type { get; set; }
    public int SourcePlayerId { get; set; }
    public int? TargetPlayerId { get; set; } // null si es efecto propio
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public int QuestionsRemaining { get; set; } = 1; // Para efectos por pregunta
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object> Properties { get; set; } = new();
}