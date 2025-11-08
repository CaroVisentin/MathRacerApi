using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para comprar un producto de la tienda
/// </summary>
public class PurchaseStoreItemUseCase
{
    private readonly IStoreRepository _storeRepository;
    private readonly IPlayerRepository _playerRepository;

    public PurchaseStoreItemUseCase(IStoreRepository storeRepository, IPlayerRepository playerRepository)
    {
        _storeRepository = storeRepository;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Procesa la compra de un producto de la tienda
    /// </summary>
    /// <param name="playerId">ID del jugador que realiza la compra</param>
    /// <param name="productId">ID del producto a comprar</param>
    /// <returns>Monedas restantes del jugador tras la compra exitosa</returns>
    /// <exception cref="NotFoundException">Se lanza cuando el jugador no existe</exception>
    /// <exception cref="BusinessException">Se lanza cuando hay un error de lógica de negocio</exception>
    public async Task<decimal> ExecuteAsync(int playerId, int productId)
    {
        // Verificar que el jugador existe
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException("Jugador no encontrado");
        }

        // Verificar que el producto existe
        var product = await _storeRepository.GetProductByIdAsync(productId, playerId);
        if (product == null)
        {
            throw new BusinessException("Producto no encontrado");
        }

        // Verificar que el jugador no ya posee el producto
        if (product.IsOwned)
        {
            throw new ConflictException("Ya posees este producto");
        }

        // Verificar que el jugador tiene suficientes monedas
        if (player.Coins < product.Price)
        {
            throw new BusinessException("No tienes suficientes monedas");
        }

        // Procesar la compra (transacción)
        var purchaseSuccessful = await _storeRepository.PurchaseProductAsync(playerId, productId, product.Price);
        
        if (!purchaseSuccessful)
        {
            throw new BusinessException("Error al procesar la compra");
        }

        // Devolver las monedas restantes
        return player.Coins - product.Price;
    }
}