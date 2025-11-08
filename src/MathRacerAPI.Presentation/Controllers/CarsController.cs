using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/cars")]

public class CarsController : ControllerBase
{
    private readonly GetStoreCarsUseCase _getStoreCarsUseCase;
    private readonly PurchaseStoreItemUseCase _purchaseStoreItemUseCase;

    public CarsController(
        GetStoreCarsUseCase getStoreCarsUseCase,
        PurchaseStoreItemUseCase purchaseStoreItemUseCase)
    {
        _getStoreCarsUseCase = getStoreCarsUseCase;
        _purchaseStoreItemUseCase = purchaseStoreItemUseCase;
    }

    [SwaggerOperation(
        Summary = "Obtiene el catálogo de autos disponibles",
        Description = "Retorna el catálogo completo de autos en la tienda con precios, rareza e información de propiedad del jugador específico",
        OperationId = "GetAllCars")]
    [SwaggerResponse(200, "Catálogo de autos obtenido exitosamente", typeof(StoreResponseDto))]
    [SwaggerResponse(400, "ID de jugador inválido")]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpGet]
    public async Task<ActionResult<StoreResponseDto>> GetAllCars([FromQuery] int playerId)
    {
        if (playerId <= 0)
        {
            throw new ValidationException("El ID del jugador debe ser mayor a 0");
        }

        var cars = await _getStoreCarsUseCase.ExecuteAsync(playerId);
        var response = cars.ToStoreResponseDto();
        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Compra un auto específico",
        Description = "Realiza la compra de un auto específico para el jugador. Valida fondos suficientes y que el jugador no posea el auto previamente.",
        OperationId = "PurchaseCar")]
    [SwaggerResponse(200, "Auto comprado exitosamente", typeof(PurchaseSuccessResponseDto))]
    [SwaggerResponse(400, "Fondos insuficientes")]
    [SwaggerResponse(404, "Jugador o auto no encontrado")]
    [SwaggerResponse(409, "El jugador ya posee este auto")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("/api/players/{playerId}/cars/{carId}")]
    public async Task<ActionResult<PurchaseSuccessResponseDto>> PurchaseCar(int playerId, int carId)
    {
        var remainingCoins = await _purchaseStoreItemUseCase.ExecuteAsync(playerId, carId);
        return Ok(remainingCoins.ToSuccessDto());
    }
}
