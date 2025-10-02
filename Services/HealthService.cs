using MathRacerAPI.Models;

namespace MathRacerAPI.Services;

/// <summary>
/// Interfaz para el servicio de health check
/// </summary>
public interface IHealthService
{
    HealthCheckResponse GetHealthStatus();
}

/// <summary>
/// Servicio para verificar el estado de salud de la aplicación
/// </summary>
public class HealthService : IHealthService
{
    public HealthCheckResponse GetHealthStatus()
    {
        var response = new HealthCheckResponse();
        
        // Aquí se pueden agregar más verificaciones
        response.Details.Add("uptime", DateTime.UtcNow);
        response.Details.Add("memory", GC.GetTotalMemory(false));
        
        return response;
    }
}