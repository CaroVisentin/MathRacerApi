using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Domain.UseCases;

public class GetStoreCarsUseCase : IGetStoreCarsUseCase
{
    private readonly IStoreRepository _storeRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetStoreCarsUseCase(IStoreRepository storeRepository, IPlayerRepository playerRepository)
    {
        _storeRepository = storeRepository;
        _playerRepository = playerRepository;
    }

    public async Task<List<StoreItem>> ExecuteAsync(int playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException($"Jugador con ID {playerId} no encontrado");
        }

        return await _storeRepository.GetProductsByTypeAsync(1, playerId);
    }
}

public class GetStoreCharactersUseCase : IGetStoreCharactersUseCase
{
    private readonly IStoreRepository _storeRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetStoreCharactersUseCase(IStoreRepository storeRepository, IPlayerRepository playerRepository)
    {
        _storeRepository = storeRepository;
        _playerRepository = playerRepository;
    }

    public async Task<List<StoreItem>> ExecuteAsync(int playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException($"Jugador con ID {playerId} no encontrado");
        }

        return await _storeRepository.GetProductsByTypeAsync(2, playerId);
    }
}

public class GetStoreBackgroundsUseCase : IGetStoreBackgroundsUseCase
{
    private readonly IStoreRepository _storeRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetStoreBackgroundsUseCase(IStoreRepository storeRepository, IPlayerRepository playerRepository)
    {
        _storeRepository = storeRepository;
        _playerRepository = playerRepository;
    }

    public async Task<List<StoreItem>> ExecuteAsync(int playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException($"Jugador con ID {playerId} no encontrado");
        }

        return await _storeRepository.GetProductsByTypeAsync(3, playerId);
    }
}

/// <summary>
/// Caso de uso para comprar un producto de la tienda
/// </summary>
public class PurchaseStoreItemUseCase : IPurchaseStoreItemUseCase
{
    private readonly IStoreRepository _storeRepository;
    private readonly IPlayerRepository _playerRepository;

    public PurchaseStoreItemUseCase(IStoreRepository storeRepository, IPlayerRepository playerRepository)
    {
        _storeRepository = storeRepository;
        _playerRepository = playerRepository;
    }

    public async Task<PurchaseResult> ExecuteAsync(int playerId, int productId)
    {
        // Verificar que el jugador existe
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException($"Jugador con ID {playerId} no encontrado");
        }

        // Verificar que el producto existe
        var product = await _storeRepository.GetProductByIdAsync(productId, playerId);
        if (product == null)
        {
            return new PurchaseResult
            {
                Success = false,
                Message = "Producto no encontrado"
            };
        }

        // Verificar que el jugador no ya posee el producto
        if (product.IsOwned)
        {
            return new PurchaseResult
            {
                Success = false,
                Message = "Ya posees este producto",
                RemainingCoins = player.Coins
            };
        }

        // Verificar que el jugador tiene suficientes monedas
        if (player.Coins < product.Price)
        {
            return new PurchaseResult
            {
                Success = false,
                Message = "No tienes suficientes monedas",
                RemainingCoins = player.Coins
            };
        }

        // Realizar la compra
        var purchaseSuccess = await _storeRepository.PurchaseProductAsync(playerId, productId, product.Price);
        
        if (purchaseSuccess)
        {
            return new PurchaseResult
            {
                Success = true,
                Message = "Compra realizada exitosamente",
                RemainingCoins = player.Coins - product.Price
            };
        }
        else
        {
            return new PurchaseResult
            {
                Success = false,
                Message = "Error al procesar la compra"
            };
        }
    }
}
