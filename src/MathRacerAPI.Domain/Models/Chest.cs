namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa un cofre con 3 items aleatorios
/// </summary>
public class Chest
{
    public List<ChestItem> Items { get; set; } = new();
}

/// <summary>
/// Item individual dentro de un cofre
/// </summary>
public class ChestItem
{
    public ChestItemType Type { get; set; }
    public int Quantity { get; set; }
    public Product? Product { get; set; }
    public Wildcard? Wildcard { get; set; }
    public int? CompensationCoins { get; set; }

    /// <summary>    /// Tipos de items que puede contener un cofre
    /// </summary>
    public enum ChestItemType
    {
        Product = 1,
        Coins = 2,
        Wildcard = 3
    }
}