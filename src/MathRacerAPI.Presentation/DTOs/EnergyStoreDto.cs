namespace MathRacerAPI.Presentation.DTOs;

/// <summary>
/// DTO para la solicitud de compra de energía
/// </summary>
public class PurchaseEnergyRequestDto
{
    /// <summary>
    /// Cantidad de energía a comprar (por defecto 1)
    /// </summary>
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// DTO para la respuesta de compra de energía
/// </summary>
public class PurchaseEnergyResponseDto
{
    /// <summary>
    /// Indica si la compra fue exitosa
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Nueva cantidad de energía del jugador
    /// </summary>
    public int NewEnergyAmount { get; set; }
    
    /// <summary>
    /// Monedas restantes del jugador
    /// </summary>
    public int RemainingCoins { get; set; }
    
    /// <summary>
    /// Precio total de la compra
    /// </summary>
    public int TotalPrice { get; set; }
}

/// <summary>
/// DTO para información de energía disponible para compra
/// </summary>
public class EnergyStoreInfoDto
{
    /// <summary>
    /// Precio por unidad de energía
    /// </summary>
    public int PricePerUnit { get; set; }
    
    /// <summary>
    /// Cantidad máxima de energía permitida
    /// </summary>
    public int MaxAmount { get; set; }
    
    /// <summary>
    /// Cantidad actual de energía del jugador
    /// </summary>
    public int CurrentAmount { get; set; }
    
    /// <summary>
    /// Cantidad máxima que puede comprar actualmente
    /// </summary>
    public int MaxCanBuy { get; set; }
}