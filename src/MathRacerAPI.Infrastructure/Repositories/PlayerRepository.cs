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
    public class PlayerRepository : IPlayerRepository
    {

        private readonly MathiRacerDbContext _context;

        public PlayerRepository(MathiRacerDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetByIdAsync(int id)
        {
           var entity = await _context.Players
                .FindAsync(id);
              if (entity == null) return null;

              return new Player
              {
                    Id = entity.Id,
                    Name = entity.Name,
              };
        }

    }
}
