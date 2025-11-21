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