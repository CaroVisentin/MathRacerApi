using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Solo;
using MathRacerAPI.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/solo")]
public class SoloController : ControllerBase
{
    private readonly StartSoloGameUseCase _startSoloGameUseCase;
    private readonly GetSoloGameStatusUseCase _getSoloGameStatusUseCase;
    private readonly SubmitSoloAnswerUseCase _submitSoloAnswerUseCase;
    private readonly UseWildcardUseCase _useWildcardUseCase;
    private readonly AbandonSoloGameUseCase _abandonSoloGameUseCase;

    public SoloController(
        StartSoloGameUseCase startSoloGameUseCase,
        GetSoloGameStatusUseCase getSoloGameStatusUseCase,
        SubmitSoloAnswerUseCase submitSoloAnswerUseCase,
        UseWildcardUseCase useWildcardUseCase,
        AbandonSoloGameUseCase abandonSoloGameUseCase)
    {
        _startSoloGameUseCase = startSoloGameUseCase;
        _getSoloGameStatusUseCase = getSoloGameStatusUseCase;
        _submitSoloAnswerUseCase = submitSoloAnswerUseCase;
        _useWildcardUseCase = useWildcardUseCase;
        _abandonSoloGameUseCase = abandonSoloGameUseCase;
    }

    [SwaggerOperation(
        Summary = "Inicia una partida individual",
        Description = "Inicia una nueva partida individual contra la máquina en el nivel especificado. Verifica energía disponible, genera 15 preguntas, asigna 3 vidas al jugador y selecciona productos para ambos competidores. Requiere que el jugador tenga 3 productos activos (auto, personaje, fondo).",
        OperationId = "StartSoloGame",
        Tags = new[] { "Solo - Modo individual" })]
    [SwaggerResponse(200, "Partida iniciada exitosamente", typeof(StartSoloGameResponseDto))]
    [SwaggerResponse(400, "Sin energía suficiente o productos incompletos")]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(404, "Nivel o mundo no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("start/{levelId}")]
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

    [SwaggerOperation(
        Summary = "Obtiene el estado de una partida individual",
        Description = "Retorna el estado completo de una partida individual en progreso, incluyendo progreso del jugador y la máquina, vidas restantes, pregunta actual y tiempo transcurrido. Actualiza automáticamente la posición de la máquina basándose en el tiempo.",
        OperationId = "GetSoloGameStatus",
        Tags = new[] { "Solo - Modo individual" })]
    [SwaggerResponse(200, "Estado obtenido exitosamente", typeof(SoloGameStatusResponseDto))]
    [SwaggerResponse(400, "Intento de acceso antes del tiempo de revisión permitido")]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(403, "El jugador no tiene permiso para acceder a esta partida")]
    [SwaggerResponse(404, "Partida no encontrada")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpGet("{gameId}")]
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

    [SwaggerOperation(
        Summary = "Envía una respuesta a la pregunta actual",
        Description = "Procesa la respuesta del jugador a la pregunta actual de la partida. Si es correcta, incrementa la posición del jugador. Si es incorrecta, reduce una vida. Otorga recompensa de monedas al completar el nivel exitosamente. Consume energía si el jugador pierde todas las vidas.",
        OperationId = "SubmitSoloAnswer",
        Tags = new[] { "Solo - Modo individual" })]
    [SwaggerResponse(200, "Respuesta procesada exitosamente", typeof(SubmitSoloAnswerResponseDto))]
    [SwaggerResponse(400, "Partida finalizada, timeout o sin preguntas disponibles")]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(403, "El jugador no tiene permiso para responder en esta partida")]
    [SwaggerResponse(404, "Partida no encontrada")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("{gameId}/answer")]
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

    [SwaggerOperation(
        Summary = "Usa un wildcard en la partida actual",
        Description = "Activa un wildcard (comodín) en la partida individual actual. Los wildcards disponibles son: 1-Eliminar opción incorrecta, 2-Saltar pregunta, 3-Doble progreso. Solo se puede usar un wildcard de cada tipo por partida y el jugador debe tener cantidad disponible.",
        OperationId = "UseWildcard",
        Tags = new[] { "Solo - Modo individual" })]
    [SwaggerResponse(200, "Wildcard usado exitosamente", typeof(UseWildcardResponseDto))]
    [SwaggerResponse(400, "Wildcard ya usado, juego finalizado o cantidad insuficiente")]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(403, "El jugador no tiene permiso para usar wildcards en esta partida")]
    [SwaggerResponse(404, "Partida o wildcard no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("{gameId}/wildcard/{wildcardId}")]
    public async Task<ActionResult<UseWildcardResponseDto>> UseWildcard(int gameId, int wildcardId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var result = await _useWildcardUseCase.ExecuteAsync(gameId, wildcardId, uid);

        return Ok(result.ToWildcardResponseDto());
    }

    [SwaggerOperation(
        Summary = "Abandona una partida individual",
        Description = "Permite al jugador abandonar una partida en progreso. La partida se marca como perdida, se consumen todas las vidas y se deduce 1 punto de energía del jugador. No se puede abandonar una partida que ya finalizó.",
        OperationId = "AbandonSoloGame",
        Tags = new[] { "Solo - Modo individual" })]
    [SwaggerResponse(200, "Partida abandonada exitosamente")]
    [SwaggerResponse(400, "Partida ya finalizada o estado inválido")]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(403, "El jugador no tiene permiso para abandonar esta partida")]
    [SwaggerResponse(404, "Partida no encontrada")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("{gameId}/abandon")]
    public async Task<ActionResult> AbandonGame(int gameId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        await _abandonSoloGameUseCase.ExecuteAsync(gameId, uid);

        return Ok(new { message = "Partida abandonada exitosamente. Energía reducida." });
    }
}
