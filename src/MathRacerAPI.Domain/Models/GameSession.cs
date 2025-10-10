using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa el estado de una sesión de juego para comunicación en tiempo real
/// </summary>
public class GameSession
{
    public int GameId { get; set; }
    public List<Player> Players { get; set; } = new();
    public GameStatus Status { get; set; }
    public Question? CurrentQuestion { get; set; }
    public int? WinnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int ConditionToWin { get; set; }
    public string? ExpectedResult { get; set; }


    /// <summary>
    /// Crea una GameSession a partir de un Game
    /// </summary>
    public static GameSession FromGame(Game game, Question? currentQuestion = null)
    {
        return new GameSession
        {
            GameId = game.Id,
            Players = game.Players,
            Status = game.Status,
            CurrentQuestion = currentQuestion,
            WinnerId = game.WinnerId,
            CreatedAt = game.CreatedAt,
            QuestionCount = game.Questions.Count,
            ConditionToWin = game.ConditionToWin,
            ExpectedResult = game.ExpectedResult
        };
    }
}