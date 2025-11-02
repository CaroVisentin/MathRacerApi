namespace MathRacerAPI.Presentation.DTOs.Chest;

/// <summary>
/// Respuesta al abrir un cofre
/// </summary>
public class ChestResponseDto
{
    public List<ChestItemDto> Items { get; set; } = new();
}

/// <summary>
/// Item del cofre
/// </summary>
public class ChestItemDto
{
    public string Type { get; set; } = string.Empty; // "Product", "Coins", "Wildcard"
    public int Quantity { get; set; }
    public ChestProductDto? Product { get; set; }
    public WildcardDto? Wildcard { get; set; } 
    public int? CompensationCoins { get; set; }
}

/// <summary>
/// Producto desbloqueado desde un cofre
/// </summary>
public class ChestProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProductType { get; set; }
    public int RarityId { get; set; }
    public string RarityName { get; set; } = string.Empty;
    public string RarityColor { get; set; } = string.Empty;
}

/// <summary>
/// Wildcard obtenido del cofre
/// </summary>
public class WildcardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}