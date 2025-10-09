using System;

namespace MathRacerAPI.Domain.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CorrectAnswers { get; set; } = 0;
    public int IndexAnswered { get; set; } = 0;
    public int Position { get; set; } = 0; 
    public bool IsReady { get; set; } = false;
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime? PenaltyUntil { get; set; }
    public DateTime? FinishedAt { get; set; }
}