namespace MathRacerAPI.Models;

/// <summary>
/// Modelo de respuesta para información general de la API
/// </summary>
public class ApiInfoResponse
{
    public string Name { get; set; } = "MathRacer API";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "API para competencias matemáticas en tiempo real";
    public string Environment { get; set; } = string.Empty;
    public ApiEndpoints Endpoints { get; set; } = new();
    public string Status { get; set; } = "Running";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Información de los endpoints disponibles
/// </summary>
public class ApiEndpoints
{
    public string Health { get; set; } = "/health";
    public string Swagger { get; set; } = "/swagger";
    public string WeatherForecast { get; set; } = "/WeatherForecast";
    public string ApiInfo { get; set; } = "/api/info";
}