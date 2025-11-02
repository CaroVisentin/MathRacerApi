using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    private readonly GetStoreCarsUseCase _getStoreCarsUseCase;
    private readonly GetStoreCharactersUseCase _getStoreCharactersUseCase;
    private readonly GetStoreBackgroundsUseCase _getStoreBackgroundsUseCase;
    private readonly PurchaseStoreItemUseCase _purchaseStoreItemUseCase;

    public StoreController(
        GetStoreCarsUseCase getStoreCarsUseCase,
        GetStoreCharactersUseCase getStoreCharactersUseCase,
        GetStoreBackgroundsUseCase getStoreBackgroundsUseCase,
        PurchaseStoreItemUseCase purchaseStoreItemUseCase)
    {
        _getStoreCarsUseCase = getStoreCarsUseCase;
        _getStoreCharactersUseCase = getStoreCharactersUseCase;
        _getStoreBackgroundsUseCase = getStoreBackgroundsUseCase;
        _purchaseStoreItemUseCase = purchaseStoreItemUseCase;
    }

    /// <summary>
    /// Obtiene todos los autos disponibles en la tienda
    /// </summary>
    /// <param name="playerId">ID del jugador para verificar propiedad de productos</param>
    /// <returns>Lista de autos con información de propiedad del jugador</returns>
    /// <response code="200">Operación exitosa. Retorna todos los autos disponibles en la tienda.</response>
    /// <response code="400">Solicitud inválida. Parámetros incorrectos.</response>
    /// <response code="404">Jugador no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/Store/cars?playerId=1
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint retorna:
    /// - **Todos los autos** disponibles en la tienda (ProductType = 1)
    /// - **Información de propiedad**: Campo `isOwned` indica si el jugador ya posee cada auto
    /// - **Precios y rareza**: Información completa de cada producto
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "items": [
    ///         {
    ///           "id": 1,
    ///           "name": "Auto Deportivo",
    ///           "description": "Un auto rápido y elegante para correr",
    ///           "price": 500.00,
    ///           "imageUrl": "",
    ///           "productTypeId": 1,
    ///           "productTypeName": "Auto",
    ///           "rarity": "Común",
    ///           "isOwned": true,
    ///           "currency": "Coins"
    ///         }
    ///       ],
    ///       "totalCount": 1
    ///     }
    /// 
    /// **Posibles errores:**
    /// 
    /// Error 400 (ValidationException - ID inválido):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El ID del jugador debe ser mayor a 0"
    ///     }
    /// 
    /// Error 404 (NotFoundException - jugador no encontrado):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Jugador no encontrado"
    ///     }
    /// 
    /// </remarks>
    [HttpGet("cars")]
    [ProducesResponseType(typeof(StoreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreResponseDto>> GetCars([FromQuery] int playerId)
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
    /// Obtiene todos los personajes disponibles en la tienda
    /// </summary>
    /// <param name="playerId">ID del jugador para verificar propiedad de productos</param>
    /// <returns>Lista de personajes con información de propiedad del jugador</returns>
    /// <response code="200">Operación exitosa. Retorna todos los personajes disponibles en la tienda.</response>
    /// <response code="400">Solicitud inválida. Parámetros incorrectos.</response>
    /// <response code="404">Jugador no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/Store/characters?playerId=1
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint retorna:
    /// - **Todos los personajes** disponibles en la tienda (ProductType = 2)
    /// - **Información de propiedad**: Campo `isOwned` indica si el jugador ya posee cada personaje
    /// - **Precios y rareza**: Información completa de cada producto
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "items": [
    ///         {
    ///           "id": 5,
    ///           "name": "Ninja Matemático",
    ///           "description": "Un personaje ágil y calculador",
    ///           "price": 800.00,
    ///           "imageUrl": "",
    ///           "productTypeId": 2,
    ///           "productTypeName": "Personaje",
    ///           "rarity": "Raro",
    ///           "isOwned": false,
    ///           "currency": "Coins"
    ///         }
    ///       ],
    ///       "totalCount": 1
    ///     }
    /// 
    /// **Posibles errores:**
    /// 
    /// Error 400 (ValidationException - ID inválido):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El ID del jugador debe ser mayor a 0"
    ///     }
    /// 
    /// Error 404 (NotFoundException - jugador no encontrado):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Jugador no encontrado"
    ///     }
    /// 
    /// </remarks>
    [HttpGet("characters")]
    [ProducesResponseType(typeof(StoreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreResponseDto>> GetCharacters([FromQuery] int playerId)
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
    /// Obtiene todos los fondos disponibles en la tienda
    /// </summary>
    /// <param name="playerId">ID del jugador para verificar propiedad de productos</param>
    /// <returns>Lista de fondos con información de propiedad del jugador</returns>
    /// <response code="200">Operación exitosa. Retorna todos los fondos disponibles en la tienda.</response>
    /// <response code="400">Solicitud inválida. Parámetros incorrectos.</response>
    /// <response code="404">Jugador no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/Store/backgrounds?playerId=1
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint retorna:
    /// - **Todos los fondos** disponibles en la tienda (ProductType = 3)
    /// - **Información de propiedad**: Campo `isOwned` indica si el jugador ya posee cada fondo
    /// - **Precios y rareza**: Información completa de cada producto
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "items": [
    ///         {
    ///           "id": 8,
    ///           "name": "Fondo Espacial",
    ///           "description": "Un hermoso paisaje galáctico de fondo",
    ///           "price": 300.00,
    ///           "imageUrl": "",
    ///           "productTypeId": 3,
    ///           "productTypeName": "Fondo",
    ///           "rarity": "Común",
    ///           "isOwned": true,
    ///           "currency": "Coins"
    ///         }
    ///       ],
    ///       "totalCount": 1
    ///     }
    /// 
    /// **Posibles errores:**
    /// 
    /// Error 400 (ValidationException - ID inválido):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El ID del jugador debe ser mayor a 0"
    ///     }
    /// 
    /// Error 404 (NotFoundException - jugador no encontrado):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Jugador no encontrado"
    ///     }
    /// 
    /// </remarks>
    [HttpGet("backgrounds")]
    [ProducesResponseType(typeof(StoreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreResponseDto>> GetBackgrounds([FromQuery] int playerId)
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
    /// Compra un producto de la tienda
    /// </summary>
    /// <param name="request">Datos de la compra (playerId y productId)</param>
    /// <returns>Resultado de la transacción de compra</returns>
    /// <response code="200">Compra realizada exitosamente.</response>
    /// <response code="400">Error en la compra (producto ya poseído, monedas insuficientes, etc.).</response>
    /// <response code="404">Jugador o producto no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/Store/purchase
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "playerId": 1,
    ///       "productId": 3
    ///     }
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint realiza la compra de un producto en la tienda:
    /// - **Valida la existencia** del jugador y producto
    /// - **Verifica la propiedad**: No permite comprar productos ya poseídos
    /// - **Valida las monedas**: Confirma que el jugador tenga suficientes fondos
    /// - **Procesa la transacción**: Descuenta monedas y otorga el producto usando transacciones ACID
    /// - **Retorna el estado**: Monedas restantes y mensaje de éxito/error
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "message": "Compra realizada exitosamente",
    ///       "remainingCoins": 750.00
    ///     }
    /// 
    /// **Posibles errores:**
    /// 
    /// Error 400 (BusinessException - monedas insuficientes):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "No tienes suficientes monedas"
    ///     }
    /// 
    /// Error 400 (BusinessException - producto no encontrado):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "Producto no encontrado"
    ///     }
    /// 
    /// Error 409 (ConflictException - producto ya poseído):
    /// 
    ///     {
    ///       "statusCode": 409,
    ///       "message": "Ya posees este producto"
    ///     }
    /// 
    /// Error 404 (NotFoundException):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Jugador no encontrado"
    ///     }
    /// 
    /// </remarks>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(PurchaseSuccessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PurchaseSuccessResponseDto>> PurchaseItem([FromBody] PurchaseRequestDto request)
    {
        var remainingCoins = await _purchaseStoreItemUseCase.ExecuteAsync(request.PlayerId, request.ProductId);

        return Ok(remainingCoins.ToSuccessDto());
    }
}
