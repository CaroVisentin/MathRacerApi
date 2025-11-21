using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MathRacerAPI.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de productos usando EF Core
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly MathiRacerDbContext _context;

    public ProductRepository(MathiRacerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene los productos activos de un jugador (auto, personaje, fondo)
    /// </summary>
    public async Task<List<PlayerProduct>> GetActiveProductsByPlayerIdAsync(int playerId)
    {
        var activeProducts = await _context.PlayerProducts
            .Where(pp => pp.PlayerId == playerId && pp.IsActive)
            .Include(pp => pp.Product)
                .ThenInclude(p => p.ProductType)
            .Include(pp => pp.Product)
                .ThenInclude(p => p.Rarity)
            .Select(pp => new PlayerProduct
            {
                ProductId = pp.Product.Id,
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

    /// <summary>
    /// Obtiene productos aleatorios para la máquina usando SQL directo para mejor rendimiento
    /// Retorna 1 producto de cada tipo (ProductTypeId 1, 2, 3)
    /// </summary>
    public async Task<List<PlayerProduct>> GetRandomProductsForMachineAsync()
    {
        // Usar SQL crudo para obtener 1 producto aleatorio de cada tipo
        // Esto es más eficiente que traer todos los productos y filtrar después
        var randomProducts = await _context.Products
            .FromSqlRaw(@"
                SELECT TOP 1 p.* FROM Product p
                WHERE p.ProductTypeId = 1
                ORDER BY NEWID()
                
                UNION ALL
                
                SELECT TOP 1 p.* FROM Product p
                WHERE p.ProductTypeId = 2
                ORDER BY NEWID()
                
                UNION ALL
                
                SELECT TOP 1 p.* FROM Product p
                WHERE p.ProductTypeId = 3
                ORDER BY NEWID()
            ")
            .Include(p => p.ProductType)
            .Include(p => p.Rarity)
            .Select(p => new PlayerProduct
            {
                ProductId = p.Id,
                Name = p.Name,
                Description = p.Description,
                ProductTypeId = p.ProductTypeId,
                ProductTypeName = p.ProductType.Name,
                RarityId = p.RarityId,
                RarityName = p.Rarity.Rarity,
                RarityColor = p.Rarity.Color
            })
            .ToListAsync();

        return randomProducts;
    }
}

