using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controller para gestionar los wildcards (comodines) del jugador
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WildcardsController : ControllerBase
{
    private readonly GetPlayerWildcardsUseCase _getPlayerWildcardsUseCase;

    public WildcardsController(GetPlayerWildcardsUseCase getPlayerWildcardsUseCase)
    {
        _getPlayerWildcardsUseCase = getPlayerWildcardsUseCase ?? throw new ArgumentNullException(nameof(getPlayerWildcardsUseCase));
    }
  
    [SwaggerOperation(
        Summary = "Obtiene los wildcards del jugador",
        Description = "Retorna la lista de wildcards (comodines) disponibles del jugador autenticado con sus cantidades. Solo incluye wildcards con cantidad mayor a 0. Los wildcards permiten ventajas especiales durante las partidas individuales.",
        OperationId = "GetPlayerWildcards",
        Tags = new[] { "Wildcards - Comodines" })]
    [SwaggerResponse(200, "Lista de wildcards obtenida exitosamente", typeof(List<PlayerWildcardDto>))]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpGet]
    public async Task<ActionResult<List<PlayerWildcardDto>>> GetMyWildcards()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var wildcards = await _getPlayerWildcardsUseCase.ExecuteByUidAsync(uid);

        var dtos = PlayerWildcardMapper.ToDtoList(wildcards);
        return Ok(dtos);
    }
}