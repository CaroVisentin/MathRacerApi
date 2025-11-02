using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
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

        public async Task UpdateAsync(PlayerProfile playerProfile)
        {
            var entity = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerProfile.Id && !p.Deleted);
            if (entity != null)
            {
                entity.Points = playerProfile.Points;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PlayerProfile?> GetByIdAsync(int id)
        {
            var entity = await _context.Players
                .Where(p => !p.Deleted)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null) return null;
            return new PlayerProfile
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Uid = entity.Uid,
                LastLevelId = entity.LastLevelId ?? 0,
                Points = entity.Points,
                Coins = entity.Coins
            };
        }

        public async Task<PlayerProfile?> GetByUidAsync(string uid)
        {
            var entity = await _context.Players
                .Where(p => !p.Deleted)
                .FirstOrDefaultAsync(p => p.Uid == uid);
            if (entity == null) return null;
            return new PlayerProfile
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Uid = entity.Uid,
                LastLevelId = entity.LastLevelId ?? 0,
                Points = entity.Points,
                Coins = entity.Coins
            };
        }

        public async Task<PlayerProfile?> GetByEmailAsync(string email)
        {
            var entity = await _context.Players
                .Where(p => !p.Deleted)
                .FirstOrDefaultAsync(p => p.Email == email);
            if (entity == null) return null;
            return new PlayerProfile
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Uid = entity.Uid,
                LastLevelId = entity.LastLevelId ?? 0,
                Points = entity.Points,
                Coins = entity.Coins
            };
        }

        public async Task<PlayerProfile> AddAsync(PlayerProfile playerProfile)
        {
            var entity = new PlayerEntity
            {
                Name = playerProfile.Name,
                Email = playerProfile.Email,
                Uid = playerProfile.Uid,
                Coins = 0,
                LastLevelId = 0,
                Points = 0,
                Deleted = false
            };
            _context.Players.Add(entity);
            await _context.SaveChangesAsync();

            var energyEntity = new EnergyEntity
            {
                PlayerId = entity.Id
            };

            _context.Energies.Add(energyEntity);
            await _context.SaveChangesAsync();

            playerProfile.Id = entity.Id;
            playerProfile.LastLevelId = entity.LastLevelId ?? 0;
            playerProfile.Points = entity.Points;
            playerProfile.Coins = entity.Coins;
            return playerProfile;
        }

        public async Task AddCoinsAsync(int playerId, int coins)
        {
            await _context.Players
                .Where(p => p.Id == playerId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Coins, p => p.Coins + coins));
        }

        public async Task UpdateLastLevelAsync(int playerId, int levelId)
        {
            await _context.Players
                .Where(p => p.Id == playerId && p.LastLevelId < levelId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.LastLevelId, levelId));
        }

    }
}