using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controller para health checks
/// </summary>
[ApiController]
[Route("[controller]")]
[Tags("Health Check")]
public class HealthController : ControllerBase
{
    private readonly GetHealthStatusUseCase _getHealthStatusUseCase;

    public HealthController(GetHealthStatusUseCase getHealthStatusUseCase)
    {
        _getHealthStatusUseCase = getHealthStatusUseCase;
    }

    /// <summary>
    /// Verifica el estado de salud de la aplicación
    /// </summary>
    /// <returns>Estado de salud detallado de la aplicación</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponseDto), StatusCodes.Status200OK)]
    public ActionResult<HealthCheckResponseDto> GetHealth()
    {
        var healthStatus = _getHealthStatusUseCase.Execute();
        
        var responseDto = new HealthCheckResponseDto
        {
            Status = healthStatus.Status,
            Timestamp = healthStatus.Timestamp,
            Version = healthStatus.Version,
            Details = healthStatus.Details
        };

        return Ok(responseDto);
    }
}