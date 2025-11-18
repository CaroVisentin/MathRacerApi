using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener todos los paquetes de monedas
/// </summary>
public class GetAllCoinPackagesUseCase
{
    private readonly ICoinPackageRepository _coinPackageRepository;

    public GetAllCoinPackagesUseCase(ICoinPackageRepository coinPackageRepository)
    {
        _coinPackageRepository = coinPackageRepository;
    }

    public async Task<List<CoinPackage>> ExecuteAsync()
    {
        return await _coinPackageRepository.GetAllAsync();
    }
}
