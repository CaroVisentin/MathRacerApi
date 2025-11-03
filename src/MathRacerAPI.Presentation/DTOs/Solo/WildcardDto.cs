namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// DTO para representar un wildcard disponible del jugador
/// </summary>
public class WildcardDto
{
    public int WildcardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
}