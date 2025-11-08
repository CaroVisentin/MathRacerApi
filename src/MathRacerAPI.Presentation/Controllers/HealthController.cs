using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("[controller]")]

public class HealthController : ControllerBase
{
    private readonly GetHealthStatusUseCase _getHealthStatusUseCase;

    public HealthController(GetHealthStatusUseCase getHealthStatusUseCase)
    {
        _getHealthStatusUseCase = getHealthStatusUseCase;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Verifica el estado de salud de la aplicación",
        Description = "Retorna el estado detallado de salud de la aplicación, incluyendo información del sistema, uptime y uso de memoria.",
        OperationId = "GetHealthStatus",
        Tags = new[] { "Health - Estado del sistema" }
    )]
    [SwaggerResponse(200, "Estado de salud de la aplicación.", typeof(HealthCheckResponseDto))]
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
