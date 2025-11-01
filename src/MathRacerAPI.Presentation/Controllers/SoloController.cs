using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Solo;
using MathRacerAPI.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controlador para el modo de juego individual
/// </summary>
[ApiController]
[Route("api/solo")]
public class SoloController : ControllerBase
{
    private readonly StartSoloGameUseCase _startSoloGameUseCase;
    private readonly GetSoloGameStatusUseCase _getSoloGameStatusUseCase;
    private readonly SubmitSoloAnswerUseCase _submitSoloAnswerUseCase;

    public SoloController(
        StartSoloGameUseCase startSoloGameUseCase,
        GetSoloGameStatusUseCase getSoloGameStatusUseCase,
        SubmitSoloAnswerUseCase submitSoloAnswerUseCase)
    {
        _startSoloGameUseCase = startSoloGameUseCase;
        _getSoloGameStatusUseCase = getSoloGameStatusUseCase;
        _submitSoloAnswerUseCase = submitSoloAnswerUseCase;
    }

    /// <summary>
    /// Inicia una nueva partida individual
    /// </summary>
    /// <param name="levelId">ID del nivel a jugar</param>
    /// <returns>Información inicial de la partida</returns>
    /// <response code="200">Partida iniciada exitosamente</response>
    /// <response code="400">No tienes energía suficiente</response>
    /// <response code="401">Token inválido</response>
    /// <response code="404">Nivel no encontrado</response>
    [HttpPost("start/{levelId}")]
    [ProducesResponseType(typeof(StartSoloGameResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StartSoloGameResponseDto>> StartGame(int levelId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var game = await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        return Ok(game.ToStartGameDto());
    }

    /// <summary>
    /// Obtiene el estado actual de una partida individual
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <returns>Estado completo de la partida</returns>
    /// <response code="200">Estado obtenido exitosamente</response>
    /// <response code="401">Token inválido o no autorizado</response>
    /// <response code="403">No tienes permiso para acceder a esta partida</response>
    /// <response code="404">Partida no encontrada</response>
    [HttpGet("{gameId}")]
    [ProducesResponseType(typeof(SoloGameStatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoloGameStatusResponseDto>> GetGameStatus(int gameId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        return Ok(result.ToStatusDto());
    }

    /// <summary>
    /// Envía una respuesta a la pregunta current
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <param name="answer">Respuesta del jugador (en el body como JSON)</param>
    /// <returns>Feedback minimalista de la respuesta</returns>
    /// <response code="200">Respuesta procesada exitosamente</response>
    /// <response code="400">Error al procesar respuesta</response>
    /// <response code="401">Token inválido</response>
    /// <response code="403">No autorizado para esta partida</response>
    /// <response code="404">Partida no encontrada</response>
    [HttpPost("{gameId}/answer")]
    [ProducesResponseType(typeof(SubmitSoloAnswerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmitSoloAnswerResponseDto>> SubmitAnswer(
        int gameId, 
        [FromBody] int answer)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        return Ok(result.ToAnswerDto());
    }
}