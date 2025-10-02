using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Models;
using MathRacerAPI.Services;

namespace MathRacerAPI.Controllers;

/// <summary>
/// Controller para información general de la API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("API Information")]
public class InfoController : ControllerBase
{
    private readonly IApiInfoService _apiInfoService;
    private readonly IWebHostEnvironment _environment;

    public InfoController(IApiInfoService apiInfoService, IWebHostEnvironment environment)
    {
        _apiInfoService = apiInfoService;
        _environment = environment;
    }

    /// <summary>
    /// Obtiene información general de la API
    /// </summary>
    /// <returns>Información de la API incluyendo versión, endpoints disponibles, etc.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiInfoResponse), StatusCodes.Status200OK)]
    public ActionResult<ApiInfoResponse> GetApiInfo()
    {
        var info = _apiInfoService.GetApiInfo(_environment.EnvironmentName);
        return Ok(info);
    }
}