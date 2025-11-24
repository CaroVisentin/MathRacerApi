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
                .Select(l => new { l.WorldId, l.Number })
                .FirstOrDefaultAsync();

            // Si no existe el nivel, retornar mundo 1 por defecto
            if (level == null)
            {
                return 1;
            }

            // Verificar si este nivel es el último del mundo
            var maxLevelNumberInWorld = await _context.Levels
                .Where(l => l.WorldId == level.WorldId)
                .MaxAsync(l => l.Number);

            // Si completó el último nivel del mundo, dar acceso al siguiente mundo
            if (level.Number == maxLevelNumberInWorld)
            {
                // Verificar si existe un mundo siguiente
                var nextWorldExists = await _context.Worlds
                    .AnyAsync(w => w.Id == level.WorldId + 1);

                return nextWorldExists ? level.WorldId + 1 : level.WorldId;
            }

            // Si no es el último nivel, permanece en el mismo mundo
            return level.WorldId;
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
