using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Chest;
using MathRacerAPI.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static MathRacerAPI.Domain.Models.ChestItem;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ChestController : ControllerBase
{
    private readonly OpenRandomChestUseCase _openRandomChest;
    private readonly OpenTutorialChestUseCase _openTutorialChest;

    public ChestController(
        OpenRandomChestUseCase openRandomChest,
        OpenTutorialChestUseCase openTutorialChest)
    {
        _openRandomChest = openRandomChest;
        _openTutorialChest = openTutorialChest;
    }

    [SwaggerOperation(
        Summary = "Abre un cofre aleatorio",
        Description = "Abre un cofre aleatorio para el jugador autenticado y otorga 3 items según probabilidades: 20% productos, 50% monedas (100-1000), 30% wildcards (1-3). Los items se aplican automáticamente a la cuenta del jugador.",
        OperationId = "OpenRandomChest",
        Tags = new[] { "Chest - Sistema de cofres" })]
    [SwaggerResponse(200, "Cofre abierto exitosamente", typeof(ChestResponseDto))]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("open")]
    public async Task<ActionResult<ChestResponseDto>> OpenRandomChest()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var chest = await _openRandomChest.ExecuteAsync(uid);

        return Ok(chest.ToResponseDto());
    }

    [SwaggerOperation(
        Summary = "Completa el tutorial y abre cofre de bienvenida",
        Description = "Marca el tutorial como completado y abre el cofre de bienvenida que contiene 3 productos comunes iniciales (1 auto, 1 personaje, 1 fondo). Los productos se asignan automáticamente como activos.",
        OperationId = "OpenTutorialChest",
        Tags = new[] { "Chest - Sistema de cofres" })]
    [SwaggerResponse(200, "Tutorial completado y cofre abierto exitosamente", typeof(ChestResponseDto))]
    [SwaggerResponse(400, "Solicitud incorrecta")]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("complete-tutorial")]
    public async Task<ActionResult<ChestResponseDto>> OpenTutorialChest()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var chest = await _openTutorialChest.ExecuteAsync(uid);

        return Ok(chest.ToResponseDto());
    }
}
