using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para comprar un cofre aleatorio con monedas
/// </summary>
public class PurchaseRandomChestUseCase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IStoreRepository _storeRepository;
    private const int CHEST_PRICE = 3000;

    public PurchaseRandomChestUseCase(
        IPlayerRepository playerRepository,
        IStoreRepository storeRepository)
    {
        _playerRepository = playerRepository;
        _storeRepository = storeRepository;
    }

    /// <summary>
    /// Procesa la compra de un cofre aleatorio
    /// </summary>
    /// <param name="uid">UID del jugador que realiza la compra</param>
    /// <returns>True si la compra fue exitosa, False en caso contrario</returns>
    /// <exception cref="NotFoundException">Se lanza cuando el jugador no existe</exception>
    /// <exception cref="InsufficientFundsException">Se lanza cuando no tiene suficientes monedas</exception>
    /// <exception cref="ValidationException">Se lanza cuando el UID es inválido</exception>
    public async Task<bool> ExecuteAsync(string uid)
    {
        // Validar UID
        if (string.IsNullOrWhiteSpace(uid))
            throw new ValidationException("El UID es requerido");

        // Verificar que el jugador existe
        var player = await _playerRepository.GetByUidAsync(uid);
        if (player == null)
        {
            throw new NotFoundException("Jugador no encontrado");
        }

        // Verificar que el jugador tiene suficientes monedas
        if (player.Coins < CHEST_PRICE)
        {
            throw new InsufficientFundsException(
                $"No tienes suficientes monedas. Necesitas {CHEST_PRICE}, tienes {player.Coins}");
        }

        // Procesar la compra (reducir monedas)
        var purchaseSuccessful = await _storeRepository.PurchaseRandomChestAsync(player.Id, CHEST_PRICE);
        
        return purchaseSuccessful;
    }
}