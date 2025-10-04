using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener información de la API
/// </summary>
public class GetApiInfoUseCase
{
    public ApiInfo Execute(string environment)
    {
        var endpoints = new ApiEndpoints();
        
        return new ApiInfo(
            name: "MathRacer API",
            version: "1.0.0",
            description: "API para competencias matemáticas en tiempo real",
            environment: environment,
            endpoints: endpoints
        );
    }
}