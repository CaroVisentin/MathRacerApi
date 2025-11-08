using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs;

namespace MathRacerAPI.Presentation.Mappers;

/// <summary>
/// Mapper para convertir entre PlayerWildcard y sus DTOs
/// </summary>
public static class PlayerWildcardMapper
{
    /// <summary>
    /// Convierte un PlayerWildcard de dominio a DTO
    /// </summary>
    public static PlayerWildcardDto ToDto(PlayerWildcard model)
    {
        return new PlayerWildcardDto
        {
            WildcardId = model.WildcardId,
            Name = model.Wildcard?.Name ?? string.Empty,
            Description = model.Wildcard?.Description ?? string.Empty,
            Quantity = model.Quantity
        };
    }

    /// <summary>
    /// Convierte una lista de PlayerWildcard de dominio a DTOs
    /// </summary>
    public static List<PlayerWildcardDto> ToDtoList(List<PlayerWildcard> models)
    {
        return models.Select(ToDto).ToList();
    }
}