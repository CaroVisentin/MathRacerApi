using MathRacerAPI.Domain.Constants;
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
                 .Include(p => p.PlayerProducts)           
                 .ThenInclude(pp => pp.Product)
                 .Include(p => p.Energy)
                 .FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null)
            {
                return null;
            }

            return MapToPlayerProfile(entity);
        }

        public async Task<PlayerProfile?> GetByUidAsync(string uid)
        {
            var entity = await _context.Players
                .Where(p => !p.Deleted)
                .Include(p => p.PlayerProducts)
                  .ThenInclude(pp => pp.Product)
                .Include(p => p.Energy)
                .FirstOrDefaultAsync(p => p.Uid == uid);

            if (entity == null)
            {
                return null;
            }

            return MapToPlayerProfile(entity);
        }

        public async Task<PlayerProfile?> GetByEmailAsync(string email)
        {
            var entity = await _context.Players
                .Where(p => !p.Deleted)
                .Include(p => p.PlayerProducts)
                  .ThenInclude(pp => pp.Product)
                .Include(p => p.Energy)
                .FirstOrDefaultAsync(p => p.Email == email);

            if (entity == null)
            {
                return null;
            }

            return MapToPlayerProfile(entity);
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
            playerProfile.EnergyStatus = new EnergyStatus
            {
                CurrentAmount = EnergyConstants.MAX_ENERGY,
                MaxAmount = EnergyConstants.MAX_ENERGY,
                SecondsUntilNextRecharge = null,
                LastCalculatedRecharge = DateTime.UtcNow
            };

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

        private PlayerProfile MapToPlayerProfile(PlayerEntity entity)
        {
            var activeProducts = entity.PlayerProducts
                .Where(pp => pp.IsActive)
                .Select(pp => pp.Product)
                .ToList();

            var carEntity = activeProducts.FirstOrDefault(p => p.ProductTypeId == 1);
            var charEntity = activeProducts.FirstOrDefault(p => p.ProductTypeId == 2);
            var bgEntity = activeProducts.FirstOrDefault(p => p.ProductTypeId == 3);

            return new PlayerProfile
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Uid = entity.Uid,
                LastLevelId = entity.LastLevelId ?? 0,
                Points = entity.Points,
                Coins = entity.Coins,
                EnergyStatus = CalculateEnergyStatus(entity.Energy),
                Car = carEntity == null ? null : new Product
                {
                    Id = carEntity.Id,
                    Name = carEntity.Name,
                    Description = carEntity.Description,
                    Price = carEntity.Price,
                    ProductType = carEntity.ProductTypeId
                },
                Background = bgEntity == null ? null : new Product
                {
                    Id = bgEntity.Id,
                    Name = bgEntity.Name,
                    Description = bgEntity.Description,
                    Price = bgEntity.Price,
                    ProductType = bgEntity.ProductTypeId
                },
                Character = charEntity == null ? null : new Product
                {
                    Id = charEntity.Id,
                    Name = charEntity.Name,
                    Description = charEntity.Description,
                    Price = charEntity.Price,
                    ProductType = charEntity.ProductTypeId
                }
            };
        }

        private EnergyStatus CalculateEnergyStatus(EnergyEntity? energy)
        {
            if (energy == null)
            {
                return new EnergyStatus
                {
                    CurrentAmount = EnergyConstants.MAX_ENERGY,
                    MaxAmount = EnergyConstants.MAX_ENERGY,
                    SecondsUntilNextRecharge = null,
                    LastCalculatedRecharge = DateTime.UtcNow
                };
            }

            if (energy.Amount >= EnergyConstants.MAX_ENERGY)
            {
                return new EnergyStatus
                {
                    CurrentAmount = EnergyConstants.MAX_ENERGY,
                    MaxAmount = EnergyConstants.MAX_ENERGY,
                    SecondsUntilNextRecharge = null,
                    LastCalculatedRecharge = DateTime.UtcNow
                };
            }

            var now = DateTime.UtcNow;
            var timeSinceLastConsumption = now - energy.LastConsumptionDate;
            var secondsPassed = (int)timeSinceLastConsumption.TotalSeconds;

            int rechargedEnergy = secondsPassed / EnergyConstants.SECONDS_PER_RECHARGE;
            int newAmount = Math.Min(energy.Amount + rechargedEnergy, EnergyConstants.MAX_ENERGY);

            int? secondsUntilNext = null;
            if (newAmount < EnergyConstants.MAX_ENERGY)
            {
                int secondsIntoCurrentCycle = secondsPassed % EnergyConstants.SECONDS_PER_RECHARGE;
                secondsUntilNext = EnergyConstants.SECONDS_PER_RECHARGE - secondsIntoCurrentCycle;
            }

            return new EnergyStatus
            {
                CurrentAmount = newAmount,
                MaxAmount = EnergyConstants.MAX_ENERGY,
                SecondsUntilNextRecharge = secondsUntilNext,
                LastCalculatedRecharge = now
            };
        }
    }
}