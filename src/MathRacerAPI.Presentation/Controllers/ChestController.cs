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
    private readonly PurchaseRandomChestUseCase _purchaseRandomChest;

    public ChestController(
        OpenRandomChestUseCase openRandomChest,
        OpenTutorialChestUseCase openTutorialChest,
        PurchaseRandomChestUseCase purchaseRandomChest)
    {
        _openRandomChest = openRandomChest;
        _openTutorialChest = openTutorialChest;
        _purchaseRandomChest = purchaseRandomChest;
    }

    [SwaggerOperation(
        Summary = "Compra y abre un cofre aleatorio",
        Description = "Permite al jugador autenticado comprar un cofre aleatorio por 3000 monedas. Valida que el jugador tenga fondos suficientes, procesa la compra y automáticamente abre el cofre, otorgando 3 items según probabilidades: 20% productos, 50% monedas (100-1000), 30% wildcards (1-3). Los items se aplican automáticamente a la cuenta del jugador.",
        OperationId = "PurchaseRandomChest",
        Tags = new[] { "Chest - Sistema de cofres" })]
    [SwaggerResponse(200, "Cofre comprado y abierto exitosamente", typeof(ChestResponseDto))]
    [SwaggerResponse(400, "Fondos insuficientes o error al procesar la compra")]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("purchase")]
    public async Task<ActionResult<ChestResponseDto>> PurchaseRandomChest()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        // 1. Intentar procesar la compra
        var purchaseSuccessful = await _purchaseRandomChest.ExecuteAsync(uid);

        // 2. Solo si la compra fue exitosa, abrir el cofre
        if (!purchaseSuccessful)
        {
            return BadRequest(new { message = "Error al procesar la compra del cofre." });
        }

        // 3. Abrir el cofre automáticamente
        var chest = await _openRandomChest.ExecuteAsync(uid);

        return Ok(chest.ToResponseDto());
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
