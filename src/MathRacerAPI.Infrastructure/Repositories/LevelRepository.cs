using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories
{
    public class LevelRepository : ILevelRepository
    {
        private readonly MathiRacerDbContext _context;

        public LevelRepository(MathiRacerDbContext context)
        {
            _context = context;
        }

        public async Task<Level?> GetByIdAsync(int id)
        {
            var entity = await _context.Levels
                .Include(l => l.ResultType)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (entity == null) return null;

            return new Level
            {
                Id = entity.Id,
                WorldId = entity.WorldId,
                Number = entity.Number,
                TermsCount = entity.TermsCount,
                VariablesCount = entity.VariablesCount,
                ResultType = entity.ResultType.Name
            };
        }

        public async Task<List<Level>> GetAllByWorldIdAsync(int worldId)
        {
            var entities = await _context.Levels
                .Include(l => l.ResultType)
                .Where(l => l.WorldId == worldId)
                .OrderBy(l => l.Number)
                .ToListAsync();

            return entities.Select(entity => new Level
            {
                Id = entity.Id,
                WorldId = entity.WorldId,
                Number = entity.Number,
                TermsCount = entity.TermsCount,
                VariablesCount = entity.VariablesCount,
                ResultType = entity.ResultType.Name
            }).ToList();
        }
    }
}
