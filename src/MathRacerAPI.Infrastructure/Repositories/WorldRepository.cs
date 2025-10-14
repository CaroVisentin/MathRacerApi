using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories
{
    public class WorldRepository : IWorldRepository
    {

        private readonly MathRacerDbContext _context;

        public WorldRepository(MathRacerDbContext context)
        {
            _context = context;
        }

        public async Task<World?> GetByIdAsync(int id)
        {
            var entity = await _context.Worlds
                 .FirstOrDefaultAsync(g => g.Id == id);

            if (entity == null) return null;

            // Mapear manualmente a dominio
            return new World
            {
                Id = entity.Id,
                Name = entity.Name,
                OptionsCount = entity.OptionsCount,
                OptionRangeMin = entity.OptionRangeMin,
                OptionRangeMax = entity.OptionRangeMax,
                NumberRangeMax = entity.NumberRangeMax,
                NumberRangeMin = entity.NumberRangeMin,
                TimePerEquation = entity.TimePerEquation,
                Difficulty = entity.Difficulty.Name, 
                Levels = entity.Levels.Select(levelEntity => new Level
                {
                    Id = levelEntity.Id,
                    WorldId = levelEntity.WorldId,
                    Number = levelEntity.Number,
                    TermsCount = levelEntity.TermsCount,
                    VariablesCount = levelEntity.VariablesCount,
                    ResultType = levelEntity.ResultType.Name 
                }).ToList(),
                Operations = entity.WorldOperations.Select(wo => wo.Operation.Sign).ToList()          
            
            };

        }
    }
}
