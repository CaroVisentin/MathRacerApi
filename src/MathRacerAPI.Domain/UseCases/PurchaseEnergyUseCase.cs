using MathRacerAPI.Domain.Constants;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para comprar energía para el juego individual
/// </summary>
public class PurchaseEnergyUseCase
{
    private readonly IEnergyRepository _energyRepository;
    private readonly IPlayerRepository _playerRepository;

    public PurchaseEnergyUseCase(IEnergyRepository energyRepository, IPlayerRepository playerRepository)
    {
        _energyRepository = energyRepository;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Procesa la compra de energía para un jugador
    /// </summary>
    /// <param name="playerId">ID del jugador que compra energía</param>
    /// <param name="quantity">Cantidad de energía a comprar (por defecto 1)</param>
    /// <returns>Nueva cantidad de energía del jugador</returns>
    /// <exception cref="NotFoundException">Se lanza cuando el jugador no existe</exception>
    /// <exception cref="BusinessException">Se lanza cuando hay un error de lógica de negocio</exception>
    public async Task<int> ExecuteAsync(int playerId, int quantity = 1)
    {
        // Verificar que el jugador existe
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException("Jugador no encontrado");
        }

        // Validar cantidad
        if (quantity <= 0)
        {
            throw new ValidationException("La cantidad debe ser mayor a cero");
        }

        // Obtener configuración de energía (precio por unidad)
        var energyConfig = await _energyRepository.GetEnergyConfigurationAsync();
        if (energyConfig == null)
        {
            throw new BusinessException("Configuración de energía no encontrada");
        }

        // Obtener energía actual del jugador
        var currentEnergyData = await _energyRepository.GetEnergyDataAsync(playerId);
        if (currentEnergyData == null)
        {
            throw new BusinessException("No se pudo obtener la información de energía del jugador");
        }

        var (currentAmount, lastConsumptionDate) = currentEnergyData.Value;

        var (pricePerUnit, maxAmount) = energyConfig.Value;

        // Verificar que no exceda el máximo permitido
        if (currentAmount + quantity > maxAmount)
        {
            var maxCanBuy = maxAmount - currentAmount;
            if (maxCanBuy <= 0)
            {
                throw new ConflictException("Ya tienes la cantidad máxima de energía permitida");
            }
            throw new ConflictException($"Solo puedes comprar {maxCanBuy} unidades de energía. Ya tienes {currentAmount}/{maxAmount}");
        }

        // Calcular precio total
        var totalPrice = pricePerUnit * quantity;

        // Verificar que el jugador tiene suficientes monedas
        if (player.Coins < totalPrice)
        {
            throw new InsufficientFundsException($"No tienes suficientes monedas. Necesitas {totalPrice}, tienes {player.Coins}");
        }

        // Procesar la compra (transacción)
        var purchaseSuccessful = await _energyRepository.PurchaseEnergyAsync(playerId, quantity, totalPrice);
        
        if (!purchaseSuccessful)
        {
            throw new BusinessException("Error al procesar la compra de energía");
        }

        // Devolver la nueva cantidad de energía
        return currentAmount + quantity;
    }
}