namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa un producto equipado por un jugador
/// </summary>
public class PlayerProduct
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProductTypeId { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public int RarityId { get; set; }
    public string RarityName { get; set; } = string.Empty;
    public string RarityColor { get; set; } = string.Empty;
}