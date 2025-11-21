using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs.Chest;
using System.Linq;

namespace MathRacerAPI.Presentation.Mappers;

/// <summary>
/// Mapper para convertir modelos de cofres a DTOs
/// </summary>
public static class ChestMapper
{
    /// <summary>
    /// Convierte un Chest del dominio a ChestResponseDto
    /// </summary>
    public static ChestResponseDto ToResponseDto(this Chest chest)
    {
        return new ChestResponseDto
        {
            Items = chest.Items.Select(item => item.ToItemDto()).ToList()
        };
    }

    /// <summary>
    /// Convierte un ChestItem del dominio a ChestItemDto
    /// </summary>
    private static ChestItemDto ToItemDto(this ChestItem item)
    {
        return new ChestItemDto
        {
            Type = item.Type.ToString(),
            Quantity = item.Quantity,
            Product = item.Product?.ToChestProductDto(),
            Wildcard = item.Wildcard?.ToChestWildcardDto(),
            CompensationCoins = item.CompensationCoins
        };
    }

    /// <summary>
    /// Convierte un Product del dominio a ChestProductDto
    /// </summary>
    private static ChestProductDto ToChestProductDto(this Product product)
    {
        return new ChestProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ProductType = product.ProductType,
            RarityId = product.RarityId,
            RarityName = product.RarityName,
            RarityColor = product.RarityColor
        };
    }

    /// <summary>
    /// Convierte un Wildcard del dominio a ChestWildcardDto
    /// </summary>
    private static ChestWildcardDto ToChestWildcardDto(this Wildcard wildcard)
    {
        return new ChestWildcardDto
        {
            Id = wildcard.Id,
            Name = wildcard.Name,
            Description = wildcard.Description
        };
    }
}