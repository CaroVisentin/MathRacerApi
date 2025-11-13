using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controller para gestionar los wildcards (comodines) del jugador y su tienda
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Wildcards - Gestión y tienda de comodines")]
public class WildcardsController : ControllerBase
{
    private readonly GetPlayerWildcardsUseCase _getPlayerWildcardsUseCase;
    private readonly GetStoreWildcardsUseCase _getStoreWildcardsUseCase;
    private readonly PurchaseWildcardUseCase _purchaseWildcardUseCase;

    public WildcardsController(
        GetPlayerWildcardsUseCase getPlayerWildcardsUseCase,
        GetStoreWildcardsUseCase getStoreWildcardsUseCase,
        PurchaseWildcardUseCase purchaseWildcardUseCase)
    {
        _getPlayerWildcardsUseCase = getPlayerWildcardsUseCase ?? throw new ArgumentNullException(nameof(getPlayerWildcardsUseCase));
        _getStoreWildcardsUseCase = getStoreWildcardsUseCase ?? throw new ArgumentNullException(nameof(getStoreWildcardsUseCase));
        _purchaseWildcardUseCase = purchaseWildcardUseCase ?? throw new ArgumentNullException(nameof(purchaseWildcardUseCase));
    }

    /// <summary>
    /// Obtiene la lista de wildcards (comodines) disponibles del jugador autenticado
    /// GET /api/wildcards
    /// Requiere token de Firebase en header Authorization
    /// </summary>
    /// <returns>Lista de wildcards del jugador con sus cantidades</returns>
    /// <response code="200">Lista de wildcards obtenida correctamente</response>
    /// <response code="401">No autorizado. Token inválido o faltante</response>
    /// <response code="404">Jugador no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/wildcards
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint:
    /// - Identifica automáticamente al jugador por su UID de Firebase
    /// - Obtiene todos los wildcards que tiene el jugador
    /// - Solo devuelve wildcards con cantidad mayor a 0
    /// - Incluye información del wildcard (nombre, descripción)
    /// 
    /// **Tipos de wildcards disponibles:**
    /// - **Matafuego**: Permite eliminar una opción incorrecta
    /// - **Cambio de rumbo**: Permite cambiar la ecuación
    /// - **Nitro**: La próxima ecuación contará como 2 ecuaciones correctas si se responde correctamente
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     [
    ///       {
    ///         "wildcardId": 1,
    ///         "name": "Matafuego",
    ///         "description": "Permite eliminar una opción incorrecta",
    ///         "quantity": 3
    ///       },
    ///       {
    ///         "wildcardId": 2,
    ///         "name": "Cambio de rumbo",
    ///         "description": "Permite cambiar la ecuación",
    ///         "quantity": 1
    ///       }
    ///     ]
    /// 
    /// **Escenarios comunes:**
    /// 
    /// 1. **Jugador con múltiples wildcards:**
    ///    ```json
    ///    [
    ///      { "wildcardId": 1, "name": "Matafuego", "description": "...", "quantity": 3 },
    ///      { "wildcardId": 2, "name": "Cambio de rumbo", "description": "...", "quantity": 1 }
    ///    ]
    ///    ```
    /// 
    /// 2. **Jugador sin wildcards (lista vacía):**
    ///    ```json
    ///    []
    ///    ```
    /// 
    /// 3. **Jugador nuevo (sin wildcards asignados):**
    ///    ```json
    ///    []
    ///    ```
    /// 
    /// **Flujo recomendado:**
    /// 
    /// 1. Usuario inicia sesión (POST /api/player/login)
    /// 2. Frontend consulta wildcards (GET /api/wildcards)
    /// 3. Frontend muestra UI con wildcards disponibles
    /// 4. Al iniciar nivel, mostrar opciones de wildcards
    /// 5. Al usar wildcard, llamar a POST /api/solo/use-wildcard
    /// 6. Actualizar cantidad localmente o volver a consultar
    /// 
    /// **Notas técnicas:**
    /// 
    /// - Los wildcards se obtienen de la tabla **PlayerWildcard**
    /// - Solo se devuelven wildcards con `Quantity > 0`
    /// - La información del wildcard viene de la tabla **Wildcard**
    /// - El sistema filtra automáticamente wildcards sin cantidad
    /// - Compatible con la lógica de uso de wildcards en partidas solo
    /// 
    /// **Integración con otros endpoints:**
    /// 
    /// - **POST /api/solo/use-wildcard**: Usa un wildcard y reduce cantidad
    /// - **POST /api/chest/open**: Puede otorgar wildcards al jugador
    /// - **GET /api/player/uid/{uid}**: Incluye información completa del jugador
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<PlayerWildcardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Obtiene todos los wildcards disponibles en la tienda junto con las cantidades actuales del jugador
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <returns>Lista de wildcards de la tienda con información del jugador</returns>
    [HttpGet("store/{playerId:int}")]
    [SwaggerOperation(Summary = "Obtener wildcards de la tienda")]
    [SwaggerResponse(200, "Lista de wildcards obtenida exitosamente", typeof(List<StoreWildcardDto>))]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    public async Task<ActionResult<List<StoreWildcardDto>>> GetStoreWildcards(int playerId)
    {
        try
        {
            var wildcards = await _getStoreWildcardsUseCase.ExecuteAsync(playerId);
            var wildcardDtos = wildcards.ToDtoList();
            return Ok(wildcardDtos);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Compra wildcards de la tienda para un jugador
    /// </summary>
    /// <param name="playerId">ID del jugador que compra</param>
    /// <param name="request">Datos de la compra (ID del wildcard y cantidad)</param>
    /// <returns>Nueva cantidad total de wildcards del jugador</returns>
    [HttpPost("purchase/{playerId:int}")]
    [SwaggerOperation(Summary = "Comprar wildcards")]
    [SwaggerResponse(200, "Compra realizada exitosamente", typeof(PurchaseResultDto))]
    [SwaggerResponse(400, "Datos de entrada inválidos")]
    [SwaggerResponse(402, "Monedas insuficientes")]
    [SwaggerResponse(404, "Jugador o wildcard no encontrado")]
    [SwaggerResponse(409, "Límite máximo de wildcards alcanzado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    public async Task<ActionResult<PurchaseResultDto>> PurchaseWildcard(int playerId, [FromBody] PurchaseWildcardRequestDto request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { message = "Datos de compra requeridos" });
            }

            var purchaseResult = await _purchaseWildcardUseCase.ExecuteAsync(playerId, request.WildcardId, request.Quantity);
            var resultDto = purchaseResult.ToPurchaseResultDto();
            return Ok(resultDto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InsufficientFundsException ex)
        {
            return StatusCode(402, new { message = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}