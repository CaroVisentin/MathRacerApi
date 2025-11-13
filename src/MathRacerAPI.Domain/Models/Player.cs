using System;

namespace MathRacerAPI.Domain.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty; // UID del jugador para matchmaking
    public int LastLevelId { get; set; } 
    public int CorrectAnswers { get; set; } = 0;
    public int IndexAnswered { get; set; } = 0;
    public int Position { get; set; } = 0; 
    public bool IsReady { get; set; } = false;
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime? PenaltyUntil { get; set; }
    public DateTime? FinishedAt { get; set; }
    public List<PowerUp> AvailablePowerUps { get; set; } = new();
    public bool HasDoublePointsActive { get; set; } = false;
}