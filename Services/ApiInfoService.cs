using MathRacerAPI.Models;

namespace MathRacerAPI.Services;

/// <summary>
/// Interfaz para el servicio de información de la API
/// </summary>
public interface IApiInfoService
{
    ApiInfoResponse GetApiInfo(string environment);
}

/// <summary>
/// Servicio para proporcionar información general de la API
/// </summary>
public class ApiInfoService : IApiInfoService
{
    public ApiInfoResponse GetApiInfo(string environment)
    {
        return new ApiInfoResponse
        {
            Environment = environment,
            Endpoints = new ApiEndpoints()
        };
    }
}