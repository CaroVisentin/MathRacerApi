namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa los wildcards que posee un jugador
/// </summary>
public class PlayerWildcard
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int WildcardId { get; set; }
    public int Quantity { get; set; }
    public Wildcard Wildcard { get; set; } = null!;
}