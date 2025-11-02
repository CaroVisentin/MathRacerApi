namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Modelo de dominio para un item de la tienda
/// </summary>
public class StoreItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int ProductTypeId { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public bool IsOwned { get; set; }
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de una operación de compra
/// </summary>
public class PurchaseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal RemainingCoins { get; set; }
}