using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Presentation.DTOs.SignalR;

/// <summary>
/// DTO para notificar el uso de un power-up
/// </summary>
public class PowerUpUsedDto
{
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public PowerUpType PowerUpType { get; set; }
    public int? TargetPlayerId { get; set; }
}