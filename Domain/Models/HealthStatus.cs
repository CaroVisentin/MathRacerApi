namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Modelo de dominio para el estado de salud de la aplicación
/// </summary>
public class HealthStatus
{
    public string Status { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Version { get; private set; }
    public Dictionary<string, object> Details { get; private set; }

    public HealthStatus(string version)
    {
        Status = "Healthy";
        Timestamp = DateTime.UtcNow;
        Version = version;
        Details = new Dictionary<string, object>();
    }

    public void AddDetail(string key, object value)
    {
        Details[key] = value;
    }

    public void SetUnhealthy(string reason)
    {
        Status = "Unhealthy";
        AddDetail("reason", reason);
    }
}