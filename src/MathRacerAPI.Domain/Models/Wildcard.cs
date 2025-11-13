namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa un wildcard/comodín en el dominio
/// </summary>
public class Wildcard
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
}

/// <summary>
/// Representa un wildcard en la tienda con información del jugador
/// </summary>
public class StoreWildcard
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public int CurrentQuantity { get; set; }
}