using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]

[Produces("application/json")]
public class OnlineController : ControllerBase
{
    private readonly IGameRepository _gameRepository;

    public OnlineController(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    [SwaggerOperation(
        Summary = "Obtiene información sobre una partida online",
        Description = "Retorna el estado completo de una partida multijugador incluyendo jugadores, progreso y configuración del juego.",
        OperationId = "GetOnlineGame",
        Tags = new[] { "Online - Multijugador" }
    )]
    [SwaggerResponse(200, "Información de la partida obtenida exitosamente.")]
    [SwaggerResponse(404, "Partida no encontrada.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpGet("game/{gameId}")]
    public async Task<IActionResult> GetGame(int gameId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        
        // Lanzar excepción personalizada si no se encuentra
        if (game == null)
        {
            throw new NotFoundException("Game", gameId);
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

    [SwaggerOperation(
        Summary = "Obtiene información de configuración para SignalR",
        Description = "Proporciona la configuración necesaria para establecer conexiones SignalR para el modo multijugador, incluyendo eventos y URL del hub.",
        OperationId = "GetSignalRConnectionInfo",
        Tags = new[] { "Online - Multijugador" }
    )]
    [SwaggerResponse(200, "Información de conexión SignalR obtenida exitosamente.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
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
