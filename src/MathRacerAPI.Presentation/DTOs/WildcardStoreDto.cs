namespace MathRacerAPI.Presentation.DTOs;

/// <summary>
/// DTO para representar un wildcard en la tienda con información del jugador
/// </summary>
public class StoreWildcardDto
{
    /// <summary>
    /// ID único del wildcard
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre del wildcard
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción del wildcard
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Precio del wildcard en monedas
    /// </summary>
    public double Price { get; set; }
    
    /// <summary>
    /// Cantidad actual que tiene el jugador (0 si no tiene)
    /// </summary>
    public int CurrentQuantity { get; set; }
}

/// <summary>
/// DTO para la solicitud de compra de wildcards
/// </summary>
public class PurchaseWildcardRequestDto
{
    /// <summary>
    /// ID del wildcard a comprar
    /// </summary>
    public int WildcardId { get; set; }
    
    /// <summary>
    /// Cantidad de wildcards a comprar (por defecto 1)
    /// </summary>
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// DTO para la respuesta de compra de wildcards
/// </summary>
public class PurchaseResultDto
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
    /// Nueva cantidad total del wildcard que tiene el jugador
    /// </summary>
    public int NewQuantity { get; set; }
}