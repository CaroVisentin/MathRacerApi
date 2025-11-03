using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MathRacerAPI.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de cofres
/// </summary>
public class ChestRepository : IChestRepository
{
    private readonly MathiRacerDbContext _context;
    private readonly Random _random;

    public ChestRepository(MathiRacerDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    public async Task<List<Product>> GetTutorialProductsAsync()
    {
        const int CommonRarityId = 1;
        var products = new List<Product>();

        // Obtener 1 producto común de cada tipo (auto, personaje, fondo)
        for (int productTypeId = 1; productTypeId <= 3; productTypeId++)
        {
            var availableProducts = await _context.Products
                .Where(p => p.RarityId == CommonRarityId && p.ProductTypeId == productTypeId)
                .Include(p => p.Rarity)
                .Include(p => p.ProductType)
                .ToListAsync();

            if (availableProducts.Any())
            {
                var randomProduct = availableProducts[_random.Next(availableProducts.Count)];
                
                products.Add(new Product
                {
                    Id = randomProduct.Id,
                    Name = randomProduct.Name,
                    Description = randomProduct.Description,
                    Price = randomProduct.Price,
                    ProductType = randomProduct.ProductTypeId,
                    RarityId = randomProduct.RarityId,
                    RarityName = randomProduct.Rarity.Rarity,
                    RarityColor = randomProduct.Rarity.Color
                });
            }
        }

        return products;
    }

    public async Task<Product?> GetRandomProductByRarityProbabilityAsync()
    {
        // 1. Obtener raridades con probabilidades
        var rarities = await _context.Rarities
            .OrderBy(r => r.Id)
            .ToListAsync();

        if (!rarities.Any()) return null;

        // 2. Seleccionar rareza según probabilidad (usar decimales 0.0-1.0)
        double roll = _random.NextDouble(); // 0.0-1.0
        double cumulative = 0;
        int selectedRarityId = 1;

        foreach (var rarity in rarities)
        {
            cumulative += rarity.Probability;
            if (roll <= cumulative)
            {
                selectedRarityId = rarity.Id;
                break;
            }
        }

        // 3. Obtener productos de esa rareza
        var products = await _context.Products
            .Where(p => p.RarityId == selectedRarityId)
            .Include(p => p.Rarity)
            .Include(p => p.ProductType)
            .ToListAsync();

        if (!products.Any()) return null;

        // 4. Seleccionar producto aleatorio
        var randomProduct = products[_random.Next(products.Count)];

        return new Product
        {
            Id = randomProduct.Id,
            Name = randomProduct.Name,
            Description = randomProduct.Description,
            Price = randomProduct.Price,
            ProductType = randomProduct.ProductTypeId,
            RarityId = randomProduct.RarityId,
            RarityName = randomProduct.Rarity.Rarity,
            RarityColor = randomProduct.Rarity.Color
        };
    }
    public async Task<Wildcard?> GetRandomWildcardAsync()
    {
        var wildcards = await _context.Wildcards
            .ToListAsync();

        if (!wildcards.Any())
            return null;

        var randomWildcard = wildcards[_random.Next(wildcards.Count)];

        return new Wildcard
        {
            Id = randomWildcard.Id,
            Name = randomWildcard.Name,
            Description = randomWildcard.Description
        };
    }

    /// <summary>
    /// Verifica si el jugador ya posee un producto
    /// </summary>
    public async Task<bool> PlayerHasProductAsync(int playerId, int productId)
    {
        return await _context.PlayerProducts
            .AnyAsync(pp => pp.PlayerId == playerId && pp.ProductId == productId);
    }

    public async Task AssignProductsToPlayerAsync(int playerId, List<int> productIds, bool setAsActive)
    {
        foreach (var productId in productIds)
        {
            var existingRelation = await _context.PlayerProducts
                .FirstOrDefaultAsync(pp => pp.PlayerId == playerId && pp.ProductId == productId);

            if (existingRelation == null)
            {
                _context.PlayerProducts.Add(new PlayerProductEntity
                {
                    PlayerId = playerId,
                    ProductId = productId,
                    IsActive = setAsActive
                });
            }
            else if (setAsActive)
            {
                existingRelation.IsActive = true;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task AddCoinsToPlayerAsync(int playerId, int coins)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player != null)
        {
            player.Coins += coins;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddWildcardsToPlayerAsync(int playerId, int wildcardId, int quantity)
    {
        var existingWildcard = await _context.PlayerWildcards
            .FirstOrDefaultAsync(pw => pw.PlayerId == playerId && pw.WildcardId == wildcardId);

        if (existingWildcard != null)
        {
            existingWildcard.Quantity += quantity;
        }
        else
        {
            _context.PlayerWildcards.Add(new PlayerWildcardEntity
            {
                PlayerId = playerId,
                WildcardId = wildcardId,
                Quantity = quantity
            });
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Obtiene los productos activos del jugador
    /// Retorna una lista de productos que el jugador tiene marcados como activos
    /// </summary>
    public async Task<List<PlayerProduct>> GetActiveProductsByPlayerIdAsync(int playerId)
    {
        var activeProducts = await _context.PlayerProducts
            .Include(pp => pp.Product)
                .ThenInclude(p => p.ProductType)
            .Include(pp => pp.Product)
                .ThenInclude(p => p.Rarity)
            .Where(pp => pp.PlayerId == playerId && pp.IsActive)
            .Select(pp => new PlayerProduct
            {
                ProductId = pp.ProductId,
                Name = pp.Product.Name,
                Description = pp.Product.Description,
                ProductTypeId = pp.Product.ProductTypeId,
                ProductTypeName = pp.Product.ProductType.Name,
                RarityId = pp.Product.RarityId,
                RarityName = pp.Product.Rarity.Rarity,
                RarityColor = pp.Product.Rarity.Color
            })
            .ToListAsync();

        return activeProducts;
    }

}