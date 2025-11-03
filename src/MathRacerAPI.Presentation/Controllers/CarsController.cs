using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// API REST para la tienda de autos - Permite consultar catálogo y realizar compras
/// </summary>
[ApiController]
[Route("api/cars")]
[Tags("Cars Store")]
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

    /// <summary>
    /// Obtiene el catálogo completo de autos disponibles en la tienda
    /// </summary>
    /// <param name="playerId">ID del jugador para verificar productos ya adquiridos</param>
    /// <returns>Catálogo de autos con precios, rareza e información de propiedad</returns>
    /// <response code="200">Catálogo obtenido exitosamente</response>
    /// <response code="400">ID de jugador inválido (debe ser mayor a 0)</response>
    /// <response code="404">Jugador no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **Endpoint REST:** `GET /api/cars?playerId={id}`
    /// 
    /// **Funcionalidad:**
    /// - Retorna todos los autos disponibles para compra
    /// - Indica cuáles ya posee el jugador (`isOwned`)
    /// - Incluye precios, descripciones y información de rareza
    /// 
    /// **Ejemplo de respuesta:**
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": 1,
    ///       "name": "Auto Deportivo Rojo",
    ///       "description": "Vehículo de alta velocidad",
    ///       "price": 150.00,
    ///       "imageUrl": "/images/cars/red-sports.png",
    ///       "productTypeId": 1,
    ///       "productTypeName": "Auto",
    ///       "rarity": "Común",
    ///       "isOwned": false,
    ///       "currency": "Coins"
    ///     }
    ///   ],
    ///   "totalCount": 12
    /// }
    /// ```
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(StoreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Compra un auto específico para el jugador
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <param name="carId">ID del auto a comprar</param>
    /// <returns>Confirmación de compra con monedas restantes</returns>
    /// <response code="200">Auto comprado exitosamente</response>
    /// <response code="400">Fondos insuficientes</response>
    /// <response code="404">Jugador o auto no encontrado</response>
    /// <response code="409">El jugador ya posee este auto</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **Endpoint REST:** `POST /api/players/{playerId}/cars/{carId}`
    /// 
    /// **Proceso de compra:**
    /// 1. Valida existencia del jugador y auto
    /// 2. Verifica que el jugador no posea el auto
    /// 3. Confirma fondos suficientes
    /// 4. Realiza la transacción y registra propiedad
    /// 
    /// **Respuesta exitosa:**
    /// ```json
    /// {
    ///   "message": "Compra realizada exitosamente",
    ///   "remainingCoins": 450.00
    /// }
    /// ```
    /// 
    /// **Códigos de error comunes:**
    /// - `400`: Fondos insuficientes
    /// - `409`: Auto ya poseído
    /// - `404`: Jugador o auto no existe
    /// </remarks>
    [HttpPost("/api/players/{playerId}/cars/{carId}")]
    [ProducesResponseType(typeof(PurchaseSuccessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PurchaseSuccessResponseDto>> PurchaseCar(int playerId, int carId)
    {
        var remainingCoins = await _purchaseStoreItemUseCase.ExecuteAsync(playerId, carId);
        return Ok(remainingCoins.ToSuccessDto());
    }
}