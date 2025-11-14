using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Infinite;
using MathRacerAPI.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InfiniteController : ControllerBase
{
    private readonly StartInfiniteGameUseCase _startInfiniteGameUseCase;
    private readonly SubmitInfiniteAnswerUseCase _submitInfiniteAnswerUseCase;
    private readonly LoadNextBatchUseCase _loadNextBatchUseCase;
    private readonly GetInfiniteGameStatusUseCase _getInfiniteGameStatusUseCase;
    private readonly AbandonInfiniteGameUseCase _abandonInfiniteGameUseCase;

    public InfiniteController(
        StartInfiniteGameUseCase startInfiniteGameUseCase,
        SubmitInfiniteAnswerUseCase submitInfiniteAnswerUseCase,
        LoadNextBatchUseCase loadNextBatchUseCase,
        GetInfiniteGameStatusUseCase getInfiniteGameStatusUseCase,
        AbandonInfiniteGameUseCase abandonInfiniteGameUseCase)
    {
        _startInfiniteGameUseCase = startInfiniteGameUseCase;
        _submitInfiniteAnswerUseCase = submitInfiniteAnswerUseCase;
        _loadNextBatchUseCase = loadNextBatchUseCase;
        _getInfiniteGameStatusUseCase = getInfiniteGameStatusUseCase;
        _abandonInfiniteGameUseCase = abandonInfiniteGameUseCase;
    }

    [SwaggerOperation(
        Summary = "Inicia una partida en modo infinito",
        Description = "Crea una nueva partida infinita con el primer lote de 9 ecuaciones. El modo infinito no tiene tiempo límite, vidas ni comodines.",
        OperationId = "StartInfiniteGame",
        Tags = new[] { "Infinite - Modo Infinito" }
    )]
    [SwaggerResponse(200, "Partida infinita iniciada exitosamente.", typeof(StartInfiniteGameResponseDto))]
    [SwaggerResponse(401, "No autorizado. Token inválido o faltante.")]
    [SwaggerResponse(404, "Jugador no encontrado.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpPost("start")]
    public async Task<IActionResult> StartInfiniteGame()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var game = await _startInfiniteGameUseCase.ExecuteAsync(uid);
        var response = InfiniteGameMapper.ToStartResponseDto(game);
        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Envía una respuesta en modo infinito",
        Description = "Procesa la respuesta del jugador para la pregunta actual y retorna si fue correcta, el total de respuestas correctas y si necesita cargar un nuevo lote.",
        OperationId = "SubmitInfiniteAnswer",
        Tags = new[] { "Infinite - Modo Infinito" }
    )]
    [SwaggerResponse(200, "Respuesta procesada exitosamente.", typeof(SubmitInfiniteAnswerResponseDto))]
    [SwaggerResponse(400, "Error de validación.")]
    [SwaggerResponse(401, "No autorizado. Token inválido o faltante.")]
    [SwaggerResponse(404, "Partida infinita no encontrada.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpPost("{gameId}/answer")]
    public async Task<IActionResult> SubmitAnswer(int gameId, [FromBody] SubmitInfiniteAnswerRequestDto request)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var result = await _submitInfiniteAnswerUseCase.ExecuteAsync(gameId, request.SelectedAnswer);
        var response = InfiniteGameMapper.ToSubmitAnswerResponseDto(result);
        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Carga el siguiente lote de ecuaciones",
        Description = "Genera y carga un nuevo lote de 9 ecuaciones con dificultad escalada. Se debe llamar cuando NeedsNewBatch sea true.",
        OperationId = "LoadNextBatch",
        Tags = new[] { "Infinite - Modo Infinito" }
    )]
    [SwaggerResponse(200, "Nuevo lote cargado exitosamente.", typeof(LoadNextBatchResponseDto))]
    [SwaggerResponse(400, "La partida no está en progreso.")]
    [SwaggerResponse(401, "No autorizado. Token inválido o faltante.")]
    [SwaggerResponse(404, "Partida infinita no encontrada.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpPost("{gameId}/load-batch")]
    public async Task<IActionResult> LoadNextBatch(int gameId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var game = await _loadNextBatchUseCase.ExecuteAsync(gameId);
        var response = InfiniteGameMapper.ToLoadNextBatchResponseDto(game);
        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Obtiene el estado actual de una partida infinita",
        Description = "Retorna información completa sobre el estado actual de la partida incluyendo progreso, respuestas correctas y lote actual.",
        OperationId = "GetInfiniteGameStatus",
        Tags = new[] { "Infinite - Modo Infinito" }
    )]
    [SwaggerResponse(200, "Estado de la partida obtenido exitosamente.", typeof(InfiniteGameStatusResponseDto))]
    [SwaggerResponse(401, "No autorizado. Token inválido o faltante.")]
    [SwaggerResponse(404, "Partida infinita no encontrada.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpGet("{gameId}/status")]
    public async Task<IActionResult> GetGameStatus(int gameId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var game = await _getInfiniteGameStatusUseCase.ExecuteAsync(gameId);
        var response = InfiniteGameMapper.ToStatusResponseDto(game);
        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Abandona una partida en modo infinito",
        Description = "Marca la partida como abandonada estableciendo la fecha de abandono. Una vez abandonada, no se pueden realizar más acciones en la partida.",
        OperationId = "AbandonInfiniteGame",
        Tags = new[] { "Infinite - Modo Infinito" }
    )]
    [SwaggerResponse(200, "Partida abandonada exitosamente.", typeof(InfiniteGameStatusResponseDto))]
    [SwaggerResponse(400, "La partida ya ha sido abandonada.")]
    [SwaggerResponse(401, "No autorizado. Token inválido o faltante.")]
    [SwaggerResponse(404, "Partida infinita no encontrada.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpPost("{gameId}/abandon")]
    public async Task<IActionResult> AbandonGame(int gameId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var game = await _abandonInfiniteGameUseCase.ExecuteAsync(gameId);
        var response = InfiniteGameMapper.ToStatusResponseDto(game);
        return Ok(response);
    }

}