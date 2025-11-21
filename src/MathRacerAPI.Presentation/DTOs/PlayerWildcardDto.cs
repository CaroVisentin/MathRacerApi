namespace MathRacerAPI.Presentation.DTOs;

/// <summary>
/// DTO para representar un wildcard del jugador
/// </summary>
public class PlayerWildcardDto
{
    public int WildcardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
}