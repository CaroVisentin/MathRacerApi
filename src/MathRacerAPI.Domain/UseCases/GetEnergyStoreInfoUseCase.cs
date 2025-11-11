using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener información de energía disponible en la tienda
/// </summary>
public class GetEnergyStoreInfoUseCase
{
    private readonly IEnergyRepository _energyRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetEnergyStoreInfoUseCase(IEnergyRepository energyRepository, IPlayerRepository playerRepository)
    {
        _energyRepository = energyRepository;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Obtiene la información de energía disponible para compra
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <returns>Información de energía de la tienda</returns>
    /// <exception cref="NotFoundException">Se lanza cuando el jugador no existe</exception>
    /// <exception cref="BusinessException">Se lanza cuando hay un error de configuración</exception>
    public async Task<(int PricePerUnit, int MaxAmount, int CurrentAmount, int MaxCanBuy)> ExecuteAsync(int playerId)
    {
        // Verificar que el jugador existe
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException("Jugador no encontrado");
        }

        // Obtener configuración de energía
        var energyConfig = await _energyRepository.GetEnergyConfigurationAsync();
        if (energyConfig == null)
        {
            throw new BusinessException("Configuración de energía no encontrada");
        }

        var (pricePerUnit, maxAmount) = energyConfig.Value;

        // Obtener energía actual del jugador
        var currentEnergyData = await _energyRepository.GetEnergyDataAsync(playerId);
        if (currentEnergyData == null)
        {
            throw new BusinessException("No se pudo obtener la información de energía del jugador");
        }

        var currentAmount = currentEnergyData.Value.Amount;

        // Calcular cuánta energía puede comprar
        var maxCanBuy = Math.Max(0, maxAmount - currentAmount);

        return (pricePerUnit, maxAmount, currentAmount, maxCanBuy);
    }
}