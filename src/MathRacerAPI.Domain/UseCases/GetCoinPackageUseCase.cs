using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener un paquete de monedas por Id
/// </summary>
public class GetCoinPackageUseCase
{
    private readonly ICoinPackageRepository _coinPackageRepository;

    public GetCoinPackageUseCase(ICoinPackageRepository coinPackageRepository)
    {
        _coinPackageRepository = coinPackageRepository;
    }

    /// <summary>
    /// Obtiene un paquete de monedas por su identificador
    /// </summary>
    /// <param name="id">Id del paquete de monedas</param>
    /// <returns>CoinPackage</returns>
    /// <exception cref="NotFoundException">Si no existe el paquete</exception>
    public async Task<CoinPackage> ExecuteAsync(int id)
    {
        var package = await _coinPackageRepository.GetByIdAsync(id);
        if (package == null)
        {
            throw new NotFoundException("Paquete de monedas no encontrado");
        }

        return package;
    }
}
