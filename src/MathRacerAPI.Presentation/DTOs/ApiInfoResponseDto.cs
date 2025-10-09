namespace MathRacerAPI.Presentation.DTOs;

/// <summary>
/// DTO de respuesta para información general de la API
/// </summary>
public class ApiInfoResponseDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public ApiEndpointsDto Endpoints { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO para información de los endpoints disponibles
/// </summary>
public class ApiEndpointsDto
{
    public string Health { get; set; } = string.Empty;
    public string Swagger { get; set; } = string.Empty;
    public string ApiInfo { get; set; } = string.Empty;
}