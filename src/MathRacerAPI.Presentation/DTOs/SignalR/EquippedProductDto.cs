namespace MathRacerAPI.Presentation.DTOs.SignalR;

/// <summary>
/// DTO para representar un producto equipado por un jugador
/// </summary>
public class EquippedProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public static EquippedProductDto? FromProduct(MathRacerAPI.Domain.Models.Product? product)
    {
        if (product == null) return null;

        return new EquippedProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description
        };
    }
}