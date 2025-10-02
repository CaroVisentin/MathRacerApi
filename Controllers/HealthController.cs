using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Models;
using MathRacerAPI.Services;

namespace MathRacerAPI.Controllers;

/// <summary>
/// Controller para health checks
/// </summary>
[ApiController]
[Route("[controller]")]
[Tags("Health Check")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    /// <summary>
    /// Verifica el estado de salud de la aplicación
    /// </summary>
    /// <returns>Estado de salud detallado de la aplicación</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthCheckResponse> GetHealth()
    {
        var health = _healthService.GetHealthStatus();
        return Ok(health);
    }
}