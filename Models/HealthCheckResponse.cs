namespace MathRacerAPI.Models;

/// <summary>
/// Modelo de respuesta para el health check
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0";
    public Dictionary<string, object> Details { get; set; } = new();
}