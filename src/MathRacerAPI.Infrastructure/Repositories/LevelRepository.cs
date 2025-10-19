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
                 .FirstOrDefaultAsync(g => g.Id == id);

            if (entity == null) return null;

            // Mapear manualmente a dominio
            return new Level
            {
                Id = entity.Id,
                WorldId = entity.WorldId,
                Number = entity.Number,
                TermsCount = entity.TermsCount,
                VariablesCount = entity.VariablesCount,
                ResultType = entity.ResultType.Name // Mapear el nombre del tipo de resultado
            };


        }
    }
}
