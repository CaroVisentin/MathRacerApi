using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using static MathRacerAPI.Domain.Models.ChestItem;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso: Abrir cofre aleatorio
/// Genera 3 items y aplica recompensas automáticamente
/// </summary>
public class OpenRandomChestUseCase
{
    private readonly IChestRepository _chestRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ChestProbabilityConfig _config;
    private readonly Random _random;

    public OpenRandomChestUseCase(
        IChestRepository chestRepository,
        IPlayerRepository playerRepository)
    {
        _chestRepository = chestRepository;
        _playerRepository = playerRepository;
        _config = new ChestProbabilityConfig();
        _random = new Random();
    }

    /// <summary>
    /// Ejecuta la apertura de un cofre aleatorio usando el UID del jugador
    /// </summary>
    /// <param name="playerUid">UID de Firebase del jugador</param>
    /// <returns>Cofre con los items generados</returns>
    public async Task<Chest> ExecuteAsync(string playerUid)
    {
        // 1. Obtener playerId desde el UID
        var playerProfile = await _playerRepository.GetByUidAsync(playerUid);
        
        if (playerProfile == null)
        {
            throw new InvalidOperationException($"Jugador con UID '{playerUid}' no encontrado.");
        }

        var items = new List<ChestItem>();

        // 2. Generar 3 items aleatorios
        for (int i = 0; i < 3; i++)
        {
            var item = await GenerateRandomItemAsync();
            
            // Verificar si es producto duplicado y aplicar compensación
            if (item.Type == ChestItemType.Product && item.Product != null)
            {
                bool isDuplicate = await _chestRepository.PlayerHasProductAsync(
                    playerProfile.Id, 
                    item.Product.Id);

                if (isDuplicate)
                {
                    // Calcular compensación según rareza
                    int compensation = _config.DuplicateCompensation.GetValueOrDefault(
                        item.Product.RarityId, 
                        50); // 50 monedas por defecto

                    item.CompensationCoins = compensation;
                    
                    // Agregar monedas como compensación
                    await _chestRepository.AddCoinsToPlayerAsync(playerProfile.Id, compensation);
                    
                }
                else
                {
                    // Producto nuevo, asignarlo
                    await ApplyRewardAsync(playerProfile.Id, item);
                }
            }
            else
            {
                // Monedas o Wildcard, aplicar normalmente
                await ApplyRewardAsync(playerProfile.Id, item);
            }

            items.Add(item);
        }

        return new Chest { Items = items };
    }

    /// <summary>
    /// Genera un item aleatorio según probabilidades
    /// </summary>
    private async Task<ChestItem> GenerateRandomItemAsync()
    {
        double roll = _random.NextDouble() * 100;

        if (roll < _config.ProductProbability)
        {
            // Producto 
            var product = await _chestRepository.GetRandomProductByRarityProbabilityAsync();
            return new ChestItem
            {
                Type = ChestItemType.Product,
                Quantity = 1,
                Product = product
            };
        }
        else if (roll < _config.ProductProbability + _config.CoinsProbability)
        {
            // Monedas
            var coins = _random.Next(_config.MinCoins, _config.MaxCoins + 1);
            return new ChestItem
            {
                Type = ChestItemType.Coins,
                Quantity = coins
            };
        }
        else
        {
            // Obtener wildcard completo con información
            var wildcard = await _chestRepository.GetRandomWildcardAsync();
            var quantity = _random.Next(_config.MinWildcards, _config.MaxWildcards + 1);
            
            return new ChestItem
            {
                Type = ChestItemType.Wildcard,
                Quantity = quantity,
                Wildcard = wildcard 
            };
        }
    }

    /// <summary>
    /// Aplica la recompensa del item al jugador (actualiza BD)
    /// </summary>
    private async Task ApplyRewardAsync(int playerId, ChestItem item)
    {
        switch (item.Type)
        {
            case ChestItemType.Product:
                if (item.Product != null)
                {
                    await _chestRepository.AssignProductsToPlayerAsync(
                        playerId, 
                        new List<int> { item.Product.Id }, 
                        setAsActive: false);
                }
                break;

            case ChestItemType.Coins:
                if (item.Quantity > 0)
                {
                    await _chestRepository.AddCoinsToPlayerAsync(playerId, item.Quantity);
                }
                break;

            case ChestItemType.Wildcard:
                if (item.Wildcard != null && item.Quantity > 0)
                {
                    await _chestRepository.AddWildcardsToPlayerAsync(
                        playerId, 
                        item.Wildcard.Id, 
                        item.Quantity);
                }
                break;
        }
    }
}