using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs;

namespace MathRacerAPI.Presentation.Mappers;

/// <summary>
/// Mapper para convertir modelos de wildcards de la tienda a DTOs
/// </summary>
public static class WildcardStoreMapper
{
    /// <summary>
    /// Convierte un StoreWildcard de dominio a StoreWildcardDto
    /// </summary>
    public static StoreWildcardDto ToDto(this StoreWildcard storeWildcard)
    {
        return new StoreWildcardDto
        {
            Id = storeWildcard.Id,
            Name = storeWildcard.Name,
            Description = storeWildcard.Description,
            Price = storeWildcard.Price,
            CurrentQuantity = storeWildcard.CurrentQuantity
        };
    }

    /// <summary>
    /// Convierte una lista de StoreWildcard a StoreWildcardDto
    /// </summary>
    public static List<StoreWildcardDto> ToDtoList(this List<StoreWildcard> storeWildcards)
    {
        return storeWildcards.Select(ToDto).ToList();
    }

    /// <summary>
    /// Convierte un resultado de compra a PurchaseResultDto
    /// </summary>
    public static PurchaseResultDto ToPurchaseResultDto(this (bool success, string message, int newQuantity, int remainingCoins) result)
    {
        return new PurchaseResultDto
        {
            Success = result.success,
            Message = result.message,
            NewQuantity = result.newQuantity,
            RemainingCoins = result.remainingCoins
        };
    }
}