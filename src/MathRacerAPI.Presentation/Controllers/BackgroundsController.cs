using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// API REST para la tienda de fondos - Permite consultar catálogo y realizar compras
/// </summary>
[ApiController]
[Route("api/backgrounds")]
[Tags("Backgrounds Store")]
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

    /// <summary>
    /// Obtiene el catálogo completo de fondos disponibles en la tienda
    /// </summary>
    /// <param name="playerId">ID del jugador para verificar fondos ya adquiridos</param>
    /// <returns>Catálogo de fondos con precios, rareza e información de propiedad</returns>
    /// <response code="200">Catálogo obtenido exitosamente</response>
    /// <response code="400">ID de jugador inválido (debe ser mayor a 0)</response>
    /// <response code="404">Jugador no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **Endpoint REST:** `GET /api/backgrounds?playerId={id}`
    /// 
    /// **Funcionalidad:**
    /// - Retorna todos los fondos disponibles para compra
    /// - Indica cuáles ya posee el jugador (`isOwned`)
    /// - Incluye precios, descripciones y información de rareza
    /// 
    /// **Ejemplo de respuesta:**
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": 3,
    ///       "name": "Galaxia Infinita",
    ///       "description": "Fondo espacial con nebulosas y estrellas",
    ///       "price": 250.00,
    ///       "imageUrl": "/images/backgrounds/galaxy.png",
    ///       "productTypeId": 3,
    ///       "productTypeName": "Fondo",
    ///       "rarity": "Épico",
    ///       "isOwned": false,
    ///       "currency": "Coins"
    ///     }
    ///   ],
    ///   "totalCount": 6
    /// }
    /// ```
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(StoreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Compra un fondo específico para el jugador
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <param name="backgroundId">ID del fondo a comprar</param>
    /// <returns>Confirmación de compra con monedas restantes</returns>
    /// <response code="200">Fondo comprado exitosamente</response>
    /// <response code="400">Fondos insuficientes</response>
    /// <response code="404">Jugador o fondo no encontrado</response>
    /// <response code="409">El jugador ya posee este fondo</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **Endpoint REST:** `POST /api/players/{playerId}/backgrounds/{backgroundId}`
    /// 
    /// **Proceso de compra:**
    /// 1. Valida existencia del jugador y fondo
    /// 2. Verifica que el jugador no posea el fondo
    /// 3. Confirma fondos suficientes
    /// 4. Realiza la transacción y registra propiedad
    /// 
    /// **Respuesta exitosa:**
    /// ```json
    /// {
    ///   "message": "Compra realizada exitosamente",
    ///   "remainingCoins": 250.00
    /// }
    /// ```
    /// 
    /// **Códigos de error comunes:**
    /// - `400`: Fondos insuficientes
    /// - `409`: Fondo ya poseído
    /// - `404`: Jugador o fondo no existe
    /// </remarks>
    [HttpPost("/api/players/{playerId}/backgrounds/{backgroundId}")]
    [ProducesResponseType(typeof(PurchaseSuccessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PurchaseSuccessResponseDto>> PurchaseBackground(int playerId, int backgroundId)
    {
        var remainingCoins = await _purchaseStoreItemUseCase.ExecuteAsync(playerId, backgroundId);
        return Ok(remainingCoins.ToSuccessDto());
    }
}