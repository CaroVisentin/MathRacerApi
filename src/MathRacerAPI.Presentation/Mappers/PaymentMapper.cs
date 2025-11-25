using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs.Payment;

namespace MathRacerAPI.Presentation.Mappers;

public static class PaymentMapper
{
    public static CoinPackageDto ToDto(this CoinPackage model)
    {
        return new CoinPackageDto
        {
            Id = model.Id,
            CoinAmount = model.CoinAmount,
            Price = model.Price,
            Description = model.Description
        };
    }

    public static List<CoinPackageDto> ToDtoList(this List<CoinPackage> models)
    {
        return models.Select(m => m.ToDto()).ToList();
    }
}
