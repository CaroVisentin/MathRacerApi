using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controller para información general de la API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("API Information")]
public class InfoController : ControllerBase
{
    private readonly GetApiInfoUseCase _getApiInfoUseCase;
    private readonly IWebHostEnvironment _environment;

    public InfoController(GetApiInfoUseCase getApiInfoUseCase, IWebHostEnvironment environment)
    {
        _getApiInfoUseCase = getApiInfoUseCase;
        _environment = environment;
    }

    /// <summary>
    /// Obtiene información general de la API
    /// </summary>
    /// <returns>Información de la API incluyendo versión, endpoints disponibles, etc.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiInfoResponseDto), StatusCodes.Status200OK)]
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