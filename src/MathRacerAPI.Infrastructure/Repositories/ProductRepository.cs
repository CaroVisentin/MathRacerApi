using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {

        private readonly MathiRacerDbContext _context;

        public ProductRepository(MathiRacerDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            var entity = await _context.Products
                 .FindAsync(id);
            if (entity == null) return null;

            // Mapear manualmente a dominio
            return new Product
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                ProductType = entity.ProductTypeId,
                Players = entity.PlayerProducts.Select(pp => new Player
                {
                    Id = pp.Player.Id,
                    Name = pp.Player.Name,
                }).ToList()
            };
        }

    }
}

