namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Modelo de dominio para información de energía de la tienda
/// </summary>
public class EnergyStoreInfo
{
    public int PricePerUnit { get; set; }
    public int MaxAmount { get; set; }
    public int CurrentAmount { get; set; }
    public int MaxCanBuy { get; set; }
}

/// <summary>
/// Modelo de dominio para el resultado de compra de energía
/// </summary>
public class EnergyPurchaseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int NewEnergyAmount { get; set; }
    public int RemainingCoins { get; set; }
    public int TotalPrice { get; set; }
}