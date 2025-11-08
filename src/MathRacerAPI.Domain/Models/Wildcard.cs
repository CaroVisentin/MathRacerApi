namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa un wildcard/comodín en el dominio
/// </summary>
public class Wildcard
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}