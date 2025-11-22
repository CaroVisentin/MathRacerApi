using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MathRacerAPI.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly MathiRacerDbContext _context;

    public StoreRepository(MathiRacerDbContext context)
    {
        _context = context;
    }

    public async Task<List<StoreItem>> GetProductsByTypeAsync(int productTypeId, int playerId)
    {
        var products = await (from p in _context.Products
                             join pt in _context.ProductTypes on p.ProductTypeId equals pt.Id
                             join r in _context.Rarities on p.RarityId equals r.Id
                             where p.ProductTypeId == productTypeId
                             select new StoreItem
                             {
                                 Id = p.Id,
                                 Name = p.Name,
                                 Description = p.Description,
                                 Price = (decimal)p.Price,
                                 ImageUrl = "", // No hay ImageUrl en la entidad actual
                                 ProductTypeId = p.ProductTypeId,
                                 ProductTypeName = pt.Name,
                                 Rarity = r.Rarity,
                                 Currency = "Coins", // Valor por defecto ya que no está en la entidad
                                 IsOwned = _context.PlayerProducts.Any(pp => pp.PlayerId == playerId && pp.ProductId == p.Id)
                             })
                             .ToListAsync();

        return products;
    }

    public async Task<StoreItem?> GetProductByIdAsync(int productId, int playerId)
    {
        var product = await (from p in _context.Products
                            join pt in _context.ProductTypes on p.ProductTypeId equals pt.Id
                            join r in _context.Rarities on p.RarityId equals r.Id
                            where p.Id == productId
                            select new StoreItem
                            {
                                Id = p.Id,
                                Name = p.Name,
                                Description = p.Description,
                                Price = (decimal)p.Price,
                                ImageUrl = "",
                                ProductTypeId = p.ProductTypeId,
                                ProductTypeName = pt.Name,
                                Rarity = r.Rarity,
                                Currency = "Coins",
                                IsOwned = _context.PlayerProducts.Any(pp => pp.PlayerId == playerId && pp.ProductId == p.Id)
                            })
                            .FirstOrDefaultAsync();

        return product;
    }

    public async Task<bool> PlayerOwnsProductAsync(int playerId, int productId)
    {
        return await _context.PlayerProducts
            .AnyAsync(pp => pp.PlayerId == playerId && pp.ProductId == productId);
    }

    public async Task<bool> PurchaseProductAsync(int playerId, int productId, decimal price)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Verificar que el jugador no posee ya el producto
            var alreadyOwns = await PlayerOwnsProductAsync(playerId, productId);
            if (alreadyOwns)
            {
                return false;
            }

            // Obtener el jugador y verificar monedas
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            if (player == null || player.Coins < (int)price)
            {
                return false;
            }

            // Descontar monedas del jugador
            player.Coins -= (int)price;

            // Agregar el producto al jugador
            var playerProduct = new PlayerProductEntity
            {
                PlayerId = playerId,
                ProductId = productId
            };

            _context.PlayerProducts.Add(playerProduct);

            // Guardar cambios
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> PurchaseRandomChestAsync(int playerId, int price)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Obtener el jugador y verificar monedas
            var player = await _context.Players
                .Where(p => p.Id == playerId && !p.Deleted)
                .FirstOrDefaultAsync();

            if (player == null || player.Coins < price)
            {
                return false;
            }

            // Descontar monedas del jugador
            player.Coins -= price;

            // Guardar cambios
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}
