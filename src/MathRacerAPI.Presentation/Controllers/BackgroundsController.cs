using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/backgrounds")]

public class BackgroundsController : ControllerBase
{
    private readonly GetStoreBackgroundsUseCase _getStoreBackgroundsUseCase;
    private readonly PurchaseStoreItemUseCase _purchaseStoreItemUseCase;

    public BackgroundsController(
        GetStoreBackgroundsUseCase getStoreBackgroundsUseCase,
        PurchaseStoreItemUseCase purchaseStoreItemUseCase)
    {
        _getStoreBackgroundsUseCase = getStoreBackgroundsUseCase;
        _purchaseStoreItemUseCase = purchaseStoreItemUseCase;
    }

    [SwaggerOperation(
        Summary = "Obtiene el catálogo de fondos disponibles",
        Description = "Retorna el catálogo completo de fondos en la tienda con precios, rareza e información de propiedad del jugador específico",
        OperationId = "GetAllBackgrounds",
        Tags = new[] { "Backgrounds - Tienda de fondos" })]
    [SwaggerResponse(200, "Catálogo de fondos obtenido exitosamente", typeof(StoreResponseDto))]
    [SwaggerResponse(400, "ID de jugador inválido")]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpGet]
    public async Task<ActionResult<StoreResponseDto>> GetAllBackgrounds([FromQuery] int playerId)
    {
        if (playerId <= 0)
        {
            throw new ValidationException("El ID del jugador debe ser mayor a 0");
        }

        var backgrounds = await _getStoreBackgroundsUseCase.ExecuteAsync(playerId);
        var response = backgrounds.ToStoreResponseDto();
        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Compra un fondo específico",
        Description = "Realiza la compra de un fondo específico para el jugador. Valida fondos suficientes y que el jugador no posea el fondo previamente.",
        OperationId = "PurchaseBackground",
        Tags = new[] { "Backgrounds - Tienda de fondos" })]
    [SwaggerResponse(200, "Fondo comprado exitosamente", typeof(PurchaseSuccessResponseDto))]
    [SwaggerResponse(400, "Fondos insuficientes")]
    [SwaggerResponse(404, "Jugador o fondo no encontrado")]
    [SwaggerResponse(409, "El jugador ya posee este fondo")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("/api/players/{playerId}/backgrounds/{backgroundId}")]
    public async Task<ActionResult<PurchaseSuccessResponseDto>> PurchaseBackground(int playerId, int backgroundId)
    {
        var remainingCoins = await _purchaseStoreItemUseCase.ExecuteAsync(playerId, backgroundId);
        return Ok(remainingCoins.ToSuccessDto());
    }
}
