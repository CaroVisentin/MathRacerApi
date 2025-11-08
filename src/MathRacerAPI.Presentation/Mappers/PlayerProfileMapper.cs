using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs;

namespace MathRacerAPI.Presentation.Mappers;

/// <summary>
/// Mapper para convertir entre PlayerProfile y sus DTOs
/// </summary>
public static class PlayerProfileMapper
{
    public static PlayerProfileDto ToDto(PlayerProfile profile)
    {
        return new PlayerProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Email = profile.Email,
            LastLevelId = profile.LastLevelId ?? 0,
            Points = profile.Points,
            Coins = profile.Coins,
            EnergyStatus = profile.EnergyStatus == null ? null : new EnergyStatusDto
            {
                CurrentAmount = profile.EnergyStatus.CurrentAmount,
                MaxAmount = profile.EnergyStatus.MaxAmount,
                SecondsUntilNextRecharge = profile.EnergyStatus.SecondsUntilNextRecharge
            },
            Car = profile.Car == null ? null : new ActiveProductDto
            {
                Id = profile.Car.Id
            },
            Character = profile.Character == null ? null : new ActiveProductDto
            {
                Id = profile.Character.Id
            },
            Background = profile.Background == null ? null : new ActiveProductDto
            {
                Id = profile.Background.Id
            }
        };
    }
}