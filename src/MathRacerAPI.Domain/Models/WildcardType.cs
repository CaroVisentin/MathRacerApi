namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Tipos de wildcards disponibles en el juego
/// </summary>
public enum WildcardType
{
    /// <summary>
    /// Elimina una opción incorrecta (ID 1)
    /// </summary>
    RemoveWrongOption = 1,
    
    /// <summary>
    /// Cambia la ecuación actual sin penalización (ID 2)
    /// </summary>
    SkipQuestion = 2,
    
    /// <summary>
    /// La siguiente respuesta correcta vale doble (ID 3)
    /// </summary>
    DoubleProgress = 3
}