using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs;

namespace MathRacerAPI.Presentation.Mappers;

/// <summary>
/// Extensiones para mapear objetos del dominio Store a DTOs
/// </summary>
public static class StoreMappers
{
    /// <summary>
    /// Mapea un StoreItem del dominio a StoreItemDto
    /// </summary>
    public static StoreItemDto ToDto(this StoreItem storeItem)
    {
        return new StoreItemDto
        {
            Id = storeItem.Id,
            Name = storeItem.Name,
            Description = storeItem.Description,
            Price = storeItem.Price,
            ImageUrl = storeItem.ImageUrl,
            ProductTypeId = storeItem.ProductTypeId,
            ProductTypeName = storeItem.ProductTypeName,
            Rarity = storeItem.Rarity,
            IsOwned = storeItem.IsOwned,
            Currency = storeItem.Currency
        };
    }

    /// <summary>
    /// Mapea una lista de StoreItems a StoreItemDtos
    /// </summary>
    public static List<StoreItemDto> ToDtoList(this List<StoreItem> storeItems)
    {
        return storeItems.Select(item => item.ToDto()).ToList();
    }

    /// <summary>
    /// Mapea una lista de StoreItems a StoreResponseDto
    /// </summary>
    public static StoreResponseDto ToStoreResponseDto(this List<StoreItem> storeItems)
    {
        return new StoreResponseDto
        {
            Items = storeItems.ToDtoList(),
            TotalCount = storeItems.Count
        };
    }

    /// <summary>
    /// Mapea un PurchaseResult del dominio a PurchaseResponseDto (backward compatibility)
    /// </summary>
    public static PurchaseResponseDto ToDto(this PurchaseResult purchaseResult)
    {
        return new PurchaseResponseDto
        {
            Success = purchaseResult.Success,
            Message = purchaseResult.Message,
            RemainingCoins = purchaseResult.RemainingCoins
        };
    }

    /// <summary>
    /// Crea una respuesta exitosa con las monedas restantes
    /// </summary>
    public static PurchaseSuccessResponseDto ToSuccessDto(this decimal remainingCoins)
    {
        return new PurchaseSuccessResponseDto
        {
            Message = "Compra realizada exitosamente",
            RemainingCoins = remainingCoins
        };
    }

    /// <summary>
    /// Mapea un PurchaseResult con error a PurchaseErrorResponseDto
    /// </summary>
    public static PurchaseErrorResponseDto ToErrorDto(this PurchaseResult purchaseResult, int statusCode)
    {
        return new PurchaseErrorResponseDto
        {
            StatusCode = statusCode,
            Message = purchaseResult.Message,
            RemainingCoins = purchaseResult.ErrorType == PurchaseErrorType.InsufficientCoins || 
                            purchaseResult.ErrorType == PurchaseErrorType.AlreadyOwned 
                            ? purchaseResult.RemainingCoins 
                            : null
        };
    }
}