using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Payment;
using MathRacerAPI.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoinsController : ControllerBase
{
    private readonly GetCoinPackageUseCase _getCoinPackageUseCase;
    private readonly GetAllCoinPackagesUseCase _getAllCoinPackagesUseCase;

    public CoinsController(
        GetCoinPackageUseCase getCoinPackageUseCase,
        GetAllCoinPackagesUseCase getAllCoinPackagesUseCase)
    {
        _getCoinPackageUseCase = getCoinPackageUseCase;
        _getAllCoinPackagesUseCase = getAllCoinPackagesUseCase;
    }

    [HttpGet("packages/{id:int}")]
    public async Task<ActionResult<CoinPackageDto>> GetCoinPackage(int id)
    {
        var cp = await _getCoinPackageUseCase.ExecuteAsync(id);
        return Ok(cp.ToDto());
    }

    [HttpGet("packages")]
    public async Task<ActionResult<List<CoinPackageDto>>> GetAllPackages()
    {
        var list = await _getAllCoinPackagesUseCase.ExecuteAsync();
        return Ok(list.ToDtoList());
    }
}
