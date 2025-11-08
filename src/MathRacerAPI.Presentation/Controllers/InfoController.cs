using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]

public class InfoController : ControllerBase
{
    private readonly GetApiInfoUseCase _getApiInfoUseCase;
    private readonly IWebHostEnvironment _environment;

    public InfoController(GetApiInfoUseCase getApiInfoUseCase, IWebHostEnvironment environment)
    {
        _getApiInfoUseCase = getApiInfoUseCase;
        _environment = environment;
    }

    [SwaggerOperation(
        Summary = "Obtiene información general de la API",
        Description = "Retorna metadatos de la API incluyendo versión, descripción, ambiente actual, estado del servicio y endpoints disponibles.",
        OperationId = "GetApiInfo",
        Tags = new[] { "Info - Información general" }
    )]
    [SwaggerResponse(200, "Información de la API obtenida exitosamente.", typeof(ApiInfoResponseDto))]
    [HttpGet]
    public ActionResult<ApiInfoResponseDto> GetApiInfo()
    {
        var apiInfo = _getApiInfoUseCase.Execute(_environment.EnvironmentName);
        
        var responseDto = new ApiInfoResponseDto
        {
            Name = apiInfo.Name,
            Version = apiInfo.Version,
            Description = apiInfo.Description,
            Environment = apiInfo.Environment,
            Status = apiInfo.Status,
            Timestamp = apiInfo.Timestamp,
            Endpoints = new ApiEndpointsDto
            {
                Health = apiInfo.Endpoints.Health,
                Swagger = apiInfo.Endpoints.Swagger,
                ApiInfo = apiInfo.Endpoints.ApiInfo
            }
        };

        return Ok(responseDto);
    }
}
