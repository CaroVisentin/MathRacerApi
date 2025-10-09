using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controlador para operaciones relacionadas con el modo multijugador online
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OnlineController : ControllerBase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<OnlineController> _logger;

    public OnlineController(IGameRepository gameRepository, ILogger<OnlineController> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene información sobre una partida online
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <returns>Información de la partida</returns>
    [HttpGet("game/{gameId}")]
    public async Task<IActionResult> GetGame(int gameId)
    {
        try
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                return NotFound(new { Message = "Partida no encontrada" });
            }

            return Ok(new
            {
                GameId = game.Id,
                Status = game.Status.ToString(),
                Players = game.Players.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CorrectAnswers,
                    p.Position,
                    p.IsReady,
                    HasPenalty = p.PenaltyUntil.HasValue && DateTime.UtcNow < p.PenaltyUntil.Value,
                    p.FinishedAt
                }),
                game.WinnerId,
                game.CreatedAt,
                QuestionCount = game.Questions.Count,
                game.ConditionToWin
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener información de la partida {gameId}");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene información sobre la conexión SignalR
    /// </summary>
    /// <returns>Información de configuración de SignalR</returns>
    [HttpGet("connection-info")]
    public IActionResult GetConnectionInfo()
    {
        return Ok(new
        {
            HubUrl = "/gameHub",
            Events = new
            {
                FindMatch = "FindMatch",
                SendAnswer = "SendAnswer",
                GameUpdate = "GameUpdate",
                Error = "Error"
            },
            Message = "Usa estos eventos para comunicarte con el GameHub de SignalR"
        });
    }
}