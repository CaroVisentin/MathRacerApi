using System;
using System.Collections.Generic;

namespace MathRacerAPI.Domain.Models;

public class Game
{
    public int Id { get; set; }
    public List<Player> Players { get; set; } = new();
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    public List<Question> Questions { get; set; } = new();
    public int MaxQuestions { get; set; } = 15;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? WinnerId { get; set; } 
    public int ConditionToWin { get; set; } = 10;
}
