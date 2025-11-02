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

public class PurchaseResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal RemainingCoins { get; set; }
}
