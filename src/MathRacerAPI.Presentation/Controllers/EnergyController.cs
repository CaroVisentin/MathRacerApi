using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnergyController : ControllerBase
{
    private readonly GetPlayerEnergyStatusUseCase _getPlayerEnergyStatusUseCase;
    private readonly PurchaseEnergyUseCase _purchaseEnergyUseCase;
    private readonly GetEnergyStoreInfoUseCase _getEnergyStoreInfoUseCase;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;

    public EnergyController(
        GetPlayerEnergyStatusUseCase getPlayerEnergyStatusUseCase,
        PurchaseEnergyUseCase purchaseEnergyUseCase,
        GetEnergyStoreInfoUseCase getEnergyStoreInfoUseCase,
        GetPlayerByIdUseCase getPlayerByIdUseCase)
    {
        _getPlayerEnergyStatusUseCase = getPlayerEnergyStatusUseCase ?? throw new ArgumentNullException(nameof(getPlayerEnergyStatusUseCase));
        _purchaseEnergyUseCase = purchaseEnergyUseCase ?? throw new ArgumentNullException(nameof(purchaseEnergyUseCase));
        _getEnergyStoreInfoUseCase = getEnergyStoreInfoUseCase ?? throw new ArgumentNullException(nameof(getEnergyStoreInfoUseCase));
        _getPlayerByIdUseCase = getPlayerByIdUseCase ?? throw new ArgumentNullException(nameof(getPlayerByIdUseCase));
    }

    [SwaggerOperation(
    Summary = "Obtiene el estado actual de energía del jugador autenticado",
        Description = "Calcula la energía actual basándose en el tiempo transcurrido desde el último consumo. Recarga automáticamente 1 punto cada 15 minutos (máximo 3). Preserva el progreso de recarga al consumir energía. Requiere token de Firebase en header Authorization.",
        OperationId = "GetEnergyStatus",
        Tags = new[] { "Energy - Estado del jugador" }
    )]
    [SwaggerResponse(200, "Estado de energía obtenido exitosamente.", typeof(EnergyStatusDto))]
    [SwaggerResponse(401, "No autorizado. Token de Firebase inválido o faltante.")]
    [SwaggerResponse(404, "Jugador no encontrado.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpGet]
    public async Task<ActionResult<EnergyStatusDto>> GetEnergyStatus()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var energyStatus = await _getPlayerEnergyStatusUseCase.ExecuteByUidAsync(uid);
        var dto = energyStatus.ToDto();
        return Ok(dto);
    }

    [SwaggerOperation(
        Summary = "Obtiene información de energía disponible en la tienda",
        Description = "Retorna el precio por unidad, cantidad máxima permitida, cantidad actual del jugador y cuánta puede comprar.",
        OperationId = "GetEnergyStoreInfo",
        Tags = new[] { "Energy - Tienda de energía" }
    )]
    [SwaggerResponse(200, "Información de energía obtenida exitosamente.", typeof(EnergyStoreInfoDto))]
    [SwaggerResponse(404, "Jugador no encontrado.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpGet("store/{playerId}")]
    public async Task<ActionResult<EnergyStoreInfoDto>> GetEnergyStoreInfo(int playerId)
    {
        var authenticatedPlayerId = await GetAuthenticatedPlayerId();

        if (playerId != authenticatedPlayerId)
            return Unauthorized("No puedes acceder a la información de tienda de otro jugador.");

        var storeInfo = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);
        var response = storeInfo.ToDto();
        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Compra energía para el juego individual",
        Description = "Permite al jugador comprar energía usando monedas. Verifica que no exceda el máximo permitido y que tenga suficientes monedas.",
        OperationId = "PurchaseEnergy",
        Tags = new[] { "Energy - Tienda de energía" }
    )]
    [SwaggerResponse(200, "Energía comprada exitosamente.", typeof(PurchaseEnergyResponseDto))]
    [SwaggerResponse(400, "Cantidad inválida - debe ser mayor a cero.")]
    [SwaggerResponse(402, "Monedas insuficientes para completar la compra.")]
    [SwaggerResponse(404, "Jugador no encontrado.")]
    [SwaggerResponse(409, "Energía máxima alcanzada - no se puede comprar más.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpPost("purchase/{playerId}")]
    public async Task<ActionResult<PurchaseEnergyResponseDto>> PurchaseEnergy(
        int playerId, 
        [FromBody] PurchaseEnergyRequestDto request)
    {
        var authenticatedPlayerId = await GetAuthenticatedPlayerId();

        if (playerId != authenticatedPlayerId)
            return Unauthorized("No puedes comprar energía para otro jugador.");

        // Procesar la compra
        var purchaseResult = await _purchaseEnergyUseCase.ExecuteAsync(playerId, request.Quantity);
        var response = purchaseResult.ToDto();
        return Ok(response);
    }

    private async Task<int> GetAuthenticatedPlayerId()
    {
        if (!HttpContext.Items.TryGetValue("FirebaseUid", out var uidObj) || uidObj == null)
            throw new UnauthorizedAccessException("Usuario no autenticado");

        var uid = uidObj.ToString();
        if (string.IsNullOrEmpty(uid))
            throw new UnauthorizedAccessException("UID de usuario inválido");
            
        var player = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);
        return player.Id;
    }
}