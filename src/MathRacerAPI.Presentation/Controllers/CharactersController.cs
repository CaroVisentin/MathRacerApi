using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// API REST para la tienda de personajes - Permite consultar catálogo y realizar compras
/// </summary>
[ApiController]
[Route("api/characters")]
[Tags("Characters Store")]
public class CharactersController : ControllerBase
{
    private readonly GetStoreCharactersUseCase _getStoreCharactersUseCase;
    private readonly PurchaseStoreItemUseCase _purchaseStoreItemUseCase;

    public CharactersController(
        GetStoreCharactersUseCase getStoreCharactersUseCase,
        PurchaseStoreItemUseCase purchaseStoreItemUseCase)
    {
        _getStoreCharactersUseCase = getStoreCharactersUseCase;
        _purchaseStoreItemUseCase = purchaseStoreItemUseCase;
    }

    /// <summary>
    /// Obtiene el catálogo completo de personajes disponibles en la tienda
    /// </summary>
    /// <param name="playerId">ID del jugador para verificar personajes ya adquiridos</param>
    /// <returns>Catálogo de personajes con precios, rareza e información de propiedad</returns>
    /// <response code="200">Catálogo obtenido exitosamente</response>
    /// <response code="400">ID de jugador inválido (debe ser mayor a 0)</response>
    /// <response code="404">Jugador no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **Endpoint REST:** `GET /api/characters?playerId={id}`
    /// 
    /// **Funcionalidad:**
    /// - Retorna todos los personajes disponibles para compra
    /// - Indica cuáles ya posee el jugador (`isOwned`)
    /// - Incluye precios, descripciones y información de rareza
    /// 
    /// **Ejemplo de respuesta:**
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": 2,
    ///       "name": "Mago Matemático",
    ///       "description": "Especialista en álgebra y geometría",
    ///       "price": 200.00,
    ///       "imageUrl": "/images/characters/wizard.png",
    ///       "productTypeId": 2,
    ///       "productTypeName": "Personaje",
    ///       "rarity": "Raro",
    ///       "isOwned": true,
    ///       "currency": "Coins"
    ///     }
    ///   ],
    ///   "totalCount": 8
    /// }
    /// ```
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(StoreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreResponseDto>> GetAllCharacters([FromQuery] int playerId)
    {
        if (playerId <= 0)
        {
            throw new ValidationException("El ID del jugador debe ser mayor a 0");
        }

        var characters = await _getStoreCharactersUseCase.ExecuteAsync(playerId);
        var response = characters.ToStoreResponseDto();
        return Ok(response);
    }

    /// <summary>
    /// Compra un personaje específico para el jugador
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <param name="characterId">ID del personaje a comprar</param>
    /// <returns>Confirmación de compra con monedas restantes</returns>
    /// <response code="200">Personaje comprado exitosamente</response>
    /// <response code="400">Fondos insuficientes</response>
    /// <response code="404">Jugador o personaje no encontrado</response>
    /// <response code="409">El jugador ya posee este personaje</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// **Endpoint REST:** `POST /api/players/{playerId}/characters/{characterId}`
    /// 
    /// **Proceso de compra:**
    /// 1. Valida existencia del jugador y personaje
    /// 2. Verifica que el jugador no posea el personaje
    /// 3. Confirma fondos suficientes
    /// 4. Realiza la transacción y registra propiedad
    /// 
    /// **Respuesta exitosa:**
    /// ```json
    /// {
    ///   "message": "Compra realizada exitosamente",
    ///   "remainingCoins": 300.00
    /// }
    /// ```
    /// 
    /// **Códigos de error comunes:**
    /// - `400`: Fondos insuficientes
    /// - `409`: Personaje ya poseído
    /// - `404`: Jugador o personaje no existe
    /// </remarks>
    [HttpPost("/api/players/{playerId}/characters/{characterId}")]
    [ProducesResponseType(typeof(PurchaseSuccessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PurchaseSuccessResponseDto>> PurchaseCharacter(int playerId, int characterId)
    {
        var remainingCoins = await _purchaseStoreItemUseCase.ExecuteAsync(playerId, characterId);
        return Ok(remainingCoins.ToSuccessDto());
    }
}