namespace MathRacerAPI.Presentation.DTOs;

public class StoreItemDto
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

public class StoreResponseDto
{
    public List<StoreItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class PurchaseRequestDto
{
    public int PlayerId { get; set; }
    public int ProductId { get; set; }
}

/// <summary>
/// Respuesta exitosa de compra (200 OK)
/// </summary>
public class PurchaseSuccessResponseDto
{
    public string Message { get; set; } = "Compra realizada exitosamente";
    public decimal RemainingCoins { get; set; }
}

/// <summary>
/// Respuesta de error de compra (400/409) - Opcional: Los errores son manejados automáticamente por el middleware
/// </summary>
public class PurchaseErrorResponseDto
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal? RemainingCoins { get; set; } // Opcional, solo para casos donde sea relevante
}

/// <summary>
/// Respuesta genérica de compra (para backward compatibility si es necesario)
/// </summary>
public class PurchaseResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal RemainingCoins { get; set; }
}
