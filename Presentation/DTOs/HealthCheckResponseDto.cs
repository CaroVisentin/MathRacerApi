namespace MathRacerAPI.Presentation.DTOs;

/// <summary>
/// DTO de respuesta para el health check
/// </summary>
public class HealthCheckResponseDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
}