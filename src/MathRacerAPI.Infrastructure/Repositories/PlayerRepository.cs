using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly MathiRacerDbContext _context;

        public PlayerRepository(MathiRacerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Player?> GetByIdAsync(int id)
        {
            var entity = await _context.Players
                .Where(p => !p.Deleted) // Excluir jugadores eliminados
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null)
            {
                return null;
            }

            return new Player
            {
                Id = entity.Id,
                Name = entity.Name,
                LastLevelId = entity.LastLevelId
            };
        }
    }
}
