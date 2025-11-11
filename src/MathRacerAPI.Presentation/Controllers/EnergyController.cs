using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnergyController : ControllerBase
{
    private readonly GetPlayerEnergyStatusUseCase _getPlayerEnergyStatusUseCase;

    public EnergyController(GetPlayerEnergyStatusUseCase getPlayerEnergyStatusUseCase)
    {
        _getPlayerEnergyStatusUseCase = getPlayerEnergyStatusUseCase ?? throw new ArgumentNullException(nameof(getPlayerEnergyStatusUseCase));
    }

    [SwaggerOperation(
        Summary = "Obtiene el estado de energía del jugador",
        Description = "Retorna el estado actual de energía del jugador autenticado, incluyendo cantidad disponible, máximo permitido y tiempo en segundos hasta la próxima recarga. La energía se recarga automáticamente 1 punto cada 15 minutos hasta un máximo de 3.",
        OperationId = "GetPlayerEnergyStatus",
        Tags = new[] { "Energy - Energía" })]
    [SwaggerResponse(200, "Estado de energía obtenido exitosamente", typeof(EnergyStatusDto))]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante")]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpGet]
    public async Task<ActionResult<EnergyStatusDto>> GetEnergyStatus()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var energyStatus = await _getPlayerEnergyStatusUseCase.ExecuteByUidAsync(uid);

        var dto = new EnergyStatusDto
        {
            CurrentAmount = energyStatus.CurrentAmount,
            MaxAmount = energyStatus.MaxAmount,
            SecondsUntilNextRecharge = energyStatus.SecondsUntilNextRecharge
        };

        return Ok(dto);
    }
}