using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para verificar el estado de salud de la aplicación
/// </summary>
public class GetHealthStatusUseCase
{
    public HealthStatus Execute()
    {
        var healthStatus = new HealthStatus("1.0.0");
        
        // Agregar detalles del estado de salud
        healthStatus.AddDetail("uptime", DateTime.UtcNow);
        healthStatus.AddDetail("memory", GC.GetTotalMemory(false));
        
        // Aquí se pueden agregar más verificaciones de salud
        // Por ejemplo, conexión a base de datos, servicios externos, etc.
        
        return healthStatus;
    }
}