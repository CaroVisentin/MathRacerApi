using System;
using System.Collections.Generic;

namespace MathRacerAPI.Domain.Models;

public class Game
{
    public int Id { get; set; }
    public List<Player> Players { get; set; } = new();
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    public List<Question> Questions { get; set; } = new();
    public int MaxQuestions { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? WinnerId { get; set; } 
    public int ConditionToWin { get; set; } = 10;
    public string ExpectedResult { get; set; } = "MAYOR";
    public bool PowerUpsEnabled { get; set; } = true;
    public List<ActiveEffect> ActiveEffects { get; set; } = new();
    public int MaxPowerUpsPerPlayer { get; set; } = 3;
}
