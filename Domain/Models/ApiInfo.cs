namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Modelo de dominio para la información de la API
/// </summary>
public class ApiInfo
{
    public string Name { get; private set; }
    public string Version { get; private set; }
    public string Description { get; private set; }
    public string Environment { get; private set; }
    public ApiEndpoints Endpoints { get; private set; }
    public string Status { get; private set; }
    public DateTime Timestamp { get; private set; }

    public ApiInfo(string name, string version, string description, string environment, ApiEndpoints endpoints)
    {
        Name = name;
        Version = version;
        Description = description;
        Environment = environment;
        Endpoints = endpoints;
        Status = "Running";
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Modelo de dominio para los endpoints de la API
/// </summary>
public class ApiEndpoints
{
    public string Health { get; private set; }
    public string Swagger { get; private set; }
    public string ApiInfo { get; private set; }

    public ApiEndpoints()
    {
        Health = "/health";
        Swagger = "/swagger";
        ApiInfo = "/api/info";
    }
}