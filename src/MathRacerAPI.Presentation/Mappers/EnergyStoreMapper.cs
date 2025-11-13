using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs;

namespace MathRacerAPI.Presentation.Mappers;

/// <summary>
/// Mapper para convertir modelos de energía de dominio a DTOs
/// </summary>
public static class EnergyStoreMapper
{
    /// <summary>
    /// Convierte EnergyStatus de dominio a EnergyStatusDto
    /// </summary>
    public static EnergyStatusDto ToDto(this EnergyStatus energyStatus)
    {
        return new EnergyStatusDto
        {
            CurrentAmount = energyStatus.CurrentAmount,
            MaxAmount = energyStatus.MaxAmount,
            SecondsUntilNextRecharge = energyStatus.SecondsUntilNextRecharge
        };
    }

    /// <summary>
    /// Convierte EnergyStoreInfo de dominio a EnergyStoreInfoDto
    /// </summary>
    public static EnergyStoreInfoDto ToDto(this EnergyStoreInfo storeInfo)
    {
        return new EnergyStoreInfoDto
        {
            PricePerUnit = storeInfo.PricePerUnit,
            MaxAmount = storeInfo.MaxAmount,
            CurrentAmount = storeInfo.CurrentAmount,
            MaxCanBuy = storeInfo.MaxCanBuy
        };
    }

    /// <summary>
    /// Convierte EnergyPurchaseResult de dominio a PurchaseEnergyResponseDto
    /// </summary>
    public static PurchaseEnergyResponseDto ToDto(this EnergyPurchaseResult purchaseResult)
    {
        return new PurchaseEnergyResponseDto
        {
            Success = purchaseResult.Success,
            Message = purchaseResult.Message,
            NewEnergyAmount = purchaseResult.NewEnergyAmount,
            RemainingCoins = purchaseResult.RemainingCoins,
            TotalPrice = purchaseResult.TotalPrice
        };
    }
}