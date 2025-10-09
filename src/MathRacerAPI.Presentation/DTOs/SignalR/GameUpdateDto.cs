using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Presentation.DTOs.SignalR;

/// <summary>
/// DTO para representar el estado del juego en SignalR
/// </summary>
public class GameUpdateDto
{
    public int GameId { get; set; }
    public List<PlayerDto> Players { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public QuestionDto? CurrentQuestion { get; set; }
    public int? WinnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int ConditionToWin { get; set; }

    /// <summary>
    /// Convierte una GameSession a GameUpdateDto
    /// </summary>
    public static GameUpdateDto FromGameSession(GameSession gameSession)
    {
        return new GameUpdateDto
        {
            GameId = gameSession.GameId,
            Players = gameSession.Players.Select(p => new PlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                CorrectAnswers = p.CorrectAnswers,
                Position = p.Position,
                IsReady = p.IsReady,
                PenaltyUntil = p.PenaltyUntil,
                FinishedAt = p.FinishedAt
            }).ToList(),
            Status = gameSession.Status.ToString(),
            CurrentQuestion = gameSession.CurrentQuestion != null ? QuestionDto.FromQuestion(gameSession.CurrentQuestion) : null,
            WinnerId = gameSession.WinnerId,
            CreatedAt = gameSession.CreatedAt,
            QuestionCount = gameSession.QuestionCount,
            ConditionToWin = gameSession.ConditionToWin
        };
    }
}