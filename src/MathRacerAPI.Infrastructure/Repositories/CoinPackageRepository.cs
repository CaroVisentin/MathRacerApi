using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MathRacerAPI.Infrastructure.Repositories
{
    public class CoinPackageRepository : ICoinPackageRepository
    {
        private readonly MathiRacerDbContext _context;
        public CoinPackageRepository(MathiRacerDbContext context)
        {
            _context = context;
        }

        public async Task<CoinPackage?> GetByIdAsync(int id)
        {
            var entity = await _context.CoinPackages.FirstOrDefaultAsync(cp => cp.Id == id);
            if (entity == null)
            {
                return null;
            }

            return new CoinPackage
            {
                Id = entity.Id,
                CoinAmount = entity.CoinAmount,
                Price = entity.Price,
                Description = entity.Description
            };
        }

        public async Task<List<CoinPackage>> GetAllAsync()
        {
            var entities = await _context.CoinPackages.AsNoTracking().ToListAsync();
            return entities.Select(e => new CoinPackage
            {
                Id = e.Id,
                CoinAmount = e.CoinAmount,
                Price = e.Price,
                Description = e.Description
            }).ToList();
        }
    }
}
