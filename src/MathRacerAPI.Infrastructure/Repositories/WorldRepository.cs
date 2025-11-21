using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories
{
    public class WorldRepository : IWorldRepository
    {
        private readonly MathiRacerDbContext _context;

        public WorldRepository(MathiRacerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<World>> GetAllWorldsAsync()
        {
            // Obtener todos los mundos (sin niveles) ordenados por ID
            var worldEntities = await _context.Worlds
                .Include(w => w.Difficulty)
                .Include(w => w.WorldOperations)
                    .ThenInclude(wo => wo.Operation)
                .OrderBy(w => w.Id)
                .ToListAsync();

            // Validar que existan mundos
            if (!worldEntities.Any())
            {
                throw new BusinessException("No hay mundos configurados en el sistema.");
            }

            // Mapear a modelos de dominio
            return worldEntities.Select(MapToWorld).ToList();
        }

        public async Task<int> GetWorldIdByLevelIdAsync(int levelId)
        {
            // Si el levelId es 0 o negativo, retornar mundo 1 (jugador nuevo)
            if (levelId <= 0)
            {
                return 1;
            }

            // Buscar el nivel y obtener su WorldId
            var level = await _context.Levels
                .Where(l => l.Id == levelId)
                .Select(l => l.WorldId)
                .FirstOrDefaultAsync();

            // Si no existe el nivel, retornar mundo 1 por defecto
            return level > 0 ? level : 1;
        }

        /// <summary>
        /// Mapea una entidad de mundo a modelo de dominio (sin niveles)
        /// </summary>
        private World MapToWorld(WorldEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return new World
            {
                Id = entity.Id,
                Name = entity.Name,
                OptionsCount = entity.OptionsCount,
                TimePerEquation = entity.TimePerEquation,
                Difficulty = entity.Difficulty?.Name ?? "Unknown",
                Operations = entity.WorldOperations?
                    .Select(wo => wo.Operation?.Sign ?? "")
                    .Where(sign => !string.IsNullOrEmpty(sign))
                    .ToList() ?? new List<string>(),
                Levels = new List<Level>() // Sin niveles
            };
        }
    }
}
