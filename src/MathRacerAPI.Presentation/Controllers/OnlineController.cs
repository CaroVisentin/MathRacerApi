using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OnlineController : ControllerBase
{
    private readonly IGameRepository _gameRepository;
    private readonly CreateCustomOnlineGameUseCase _createCustomGameUseCase;
    private readonly GetAvailableGamesUseCase _getAvailableGamesUseCase;
    private readonly JoinCreatedGameUseCase _joinCreatedGameUseCase;

    public OnlineController(
        IGameRepository gameRepository,
        CreateCustomOnlineGameUseCase createCustomGameUseCase,
        GetAvailableGamesUseCase getAvailableGamesUseCase,
        JoinCreatedGameUseCase joinCreatedGameUseCase)
    {
        _gameRepository = gameRepository;
        _createCustomGameUseCase = createCustomGameUseCase;
        _getAvailableGamesUseCase = getAvailableGamesUseCase;
        _joinCreatedGameUseCase = joinCreatedGameUseCase;
    }

    [SwaggerOperation(
        Summary = "Obtiene la lista de partidas disponibles",
        Description = "Retorna una lista de todas las partidas multijugador que están esperando jugadores. Opcionalmente se pueden filtrar solo partidas públicas.",
        OperationId = "GetAvailableGames",
        Tags = new[] { "Online - Multijugador" }
    )]
    [SwaggerResponse(200, "Lista de partidas obtenida exitosamente.", typeof(AvailableGamesResponseDto))]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpGet("games/available")]
    public async Task<ActionResult<AvailableGamesResponseDto>> GetAvailableGames(
        [FromQuery] bool publicOnly = false)
    {
        var games = await _getAvailableGamesUseCase.ExecuteAsync(includePrivate: !publicOnly);

        var responseDto = new AvailableGamesResponseDto
        {
            Games = games.Select(g => new AvailableGameDto
            {
                GameId = g.GameId,
                GameName = g.GameName,
                IsPrivate = g.IsPrivate,
                RequiresPassword = g.RequiresPassword,
                CurrentPlayers = g.CurrentPlayers,
                MaxPlayers = g.MaxPlayers,
                Difficulty = g.Difficulty,
                ExpectedResult = g.ExpectedResult,
                CreatedAt = g.CreatedAt,
                CreatorName = g.CreatorName,
                IsFull = g.IsFull,
                Status = "Esperando jugadores"
            }).ToList(),
            TotalGames = games.Count,
            PublicGames = games.Count(g => !g.IsPrivate),
            PrivateGames = games.Count(g => g.IsPrivate),
            Timestamp = DateTime.UtcNow
        };

        return Ok(responseDto);
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
        
        if (game == null)
        {
            throw new NotFoundException("Game", gameId);
        }

        return Ok(new
        {
            GameId = game.Id,
            GameName = game.Name,
            IsPrivate = game.IsPrivate,
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
            game.ConditionToWin,
            game.ExpectedResult,
            game.CreatorPlayerId
        });
    }

    [SwaggerOperation(
        Summary = "Crea una partida multijugador personalizada",
        Description = "Permite crear una partida online personalizada con configuración de dificultad, privacidad y resultado esperado. Requiere autenticación con Firebase.",
        OperationId = "CreateCustomOnlineGame",
        Tags = new[] { "Online - Multijugador" }
    )]
    [SwaggerResponse(201, "Partida creada exitosamente.")]
    [SwaggerResponse(400, "Solicitud inválida o parámetros incorrectos.")]
    [SwaggerResponse(401, "No autorizado. Token de Firebase requerido.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateCustomGame([FromBody] CreateCustomGameRequestDto request)
    {
        var firebaseUid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(firebaseUid))
        {
            return Unauthorized(new { error = "Token de autenticación requerido o inválido." });
        }

        if (string.IsNullOrWhiteSpace(request.GameName))
        {
            return BadRequest(new { error = "El nombre de la partida es requerido." });
        }

        string connectionId = Guid.NewGuid().ToString();

        var game = await _createCustomGameUseCase.ExecuteAsync(
            firebaseUid,
            request.GameName,
            connectionId,
            request.IsPrivate,
            request.Password,
            request.Difficulty,
            request.ExpectedResult
        );

        return CreatedAtAction(
            nameof(GetGame),
            new { gameId = game.Id },
            new
            {
                GameId = game.Id,
                GameName = game.Name,
                IsPrivate = game.IsPrivate,
                Status = game.Status.ToString(),
                Players = game.Players.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CorrectAnswers,
                    p.Position,
                    p.IsReady,
                    AvailablePowerUps = p.AvailablePowerUps.Select(pu => new
                    {
                        pu.Id,
                        Type = (int)pu.Type,
                        pu.Name,
                        pu.Description
                    })
                }),
                game.CreatedAt,
                QuestionCount = game.Questions.Count,
                game.ConditionToWin,
                game.ExpectedResult,
                game.PowerUpsEnabled,
                game.CreatorPlayerId,
                Message = "Partida creada exitosamente. Esperando jugadores..."
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
                JoinGame = "JoinGame",
                SendAnswer = "SendAnswer",
                GameUpdate = "GameUpdate",
                Error = "Error"
            },
            Message = "Usa estos eventos para comunicarte con el GameHub de SignalR"
        });
    }
}
