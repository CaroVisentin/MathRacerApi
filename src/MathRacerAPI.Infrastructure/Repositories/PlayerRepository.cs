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

        public async Task<PlayerProfile?> GetByIdAsync(int id)
        {
         

            var entity = await _context.Players
                 .Where(p => !p.Deleted)
                 .Include(p => p.PlayerProducts)           
                 .ThenInclude(pp => pp.Product)  
                 .FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null)
            {
                return null;
            }
            var activeProducts = entity.PlayerProducts
         .Where(pp => pp.IsActive)
         .Select(pp => pp.Product)
         .ToList();

            // Mapeo manual de cada tipo de producto

            var carEntity = activeProducts.FirstOrDefault(p => p.ProductTypeId == 1);
            var charEntity = activeProducts.FirstOrDefault(p => p.ProductTypeId == 2);
            var bgEntity = activeProducts.FirstOrDefault(p => p.ProductTypeId == 3);

            return new PlayerProfile
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Uid = entity.Uid,
                LastLevelId = entity.LastLevelId,
                Points = entity.Points,
                Coins = entity.Coins,

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

        public async Task<PlayerProfile?> GetByUidAsync(string uid)
        {
            var entity = await _context.Players
                .Where(p => !p.Deleted)
                .Include(p => p.PlayerProducts)
                  .ThenInclude(pp => pp.Product)
                .FirstOrDefaultAsync(p => p.Uid == uid);

            if (entity == null)
            {
                return null;
            }


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
                LastLevelId = entity.LastLevelId,
                Points = entity.Points,
                Coins = entity.Coins,

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

        public async Task<PlayerProfile?> GetByEmailAsync(string email)
        {
            var entity = await _context.Players
                .Where(p => !p.Deleted)
                .Include(p => p.PlayerProducts)
                  .ThenInclude(pp => pp.Product)
                .FirstOrDefaultAsync(p => p.Email == email);

            if (entity == null)
            {
                return null;
            }

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
                LastLevelId = entity.LastLevelId,
                Points = entity.Points,
                Coins = entity.Coins,

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

        public async Task<PlayerProfile> AddAsync(PlayerProfile playerProfile)
        {
            var entity = new PlayerEntity
            {
                Name = playerProfile.Name,
                Email = playerProfile.Email,
                Uid = playerProfile.Uid,
                Coins = 0,
                LastLevelId = 1,
                Points = 0,
                Deleted = false
            };

            _context.Players.Add(entity);
            await _context.SaveChangesAsync();

            var defaultCar = await _context.Products
       .FirstOrDefaultAsync(p => p.ProductTypeId == 1 && p.Id == 1);

            var defaultCharacter = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductTypeId == 2 && p.Id == 2);

            var defaultBackground = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductTypeId == 3 && p.Id == 3);

            //  Asignar los productos al jugador

            var defaultProducts = new List<PlayerProductEntity>();

            if (defaultCar != null)
            {
                defaultProducts.Add(new PlayerProductEntity
                {
                    PlayerId = entity.Id,
                    ProductId = defaultCar.Id,
                    IsActive = true 
                });
            }


            if (defaultCharacter != null)
            {
                defaultProducts.Add(new PlayerProductEntity
                {
                    PlayerId = entity.Id,
                    ProductId = defaultCharacter.Id,
                    IsActive = true
                });
            }


            if (defaultBackground != null)
            {
                defaultProducts.Add(new PlayerProductEntity
                {
                    PlayerId = entity.Id,
                    ProductId = defaultBackground.Id,
                    IsActive = true
                });
            }

            if (defaultProducts.Any())
            {
                _context.PlayerProducts.AddRange(defaultProducts);
                await _context.SaveChangesAsync();
            }

            playerProfile.Id = entity.Id;
            playerProfile.LastLevelId = entity.LastLevelId;
            playerProfile.Points = entity.Points;
            playerProfile.Coins = entity.Coins;

            playerProfile.Car = defaultCar == null ? null : new Product
            {
                Id = defaultCar.Id,
                Name = defaultCar.Name,
                Description = defaultCar.Description,
                Price = defaultCar.Price,
                ProductType = defaultCar.ProductTypeId
            };

            playerProfile.Background = defaultBackground == null ? null : new Product
            {
                Id = defaultBackground.Id,
                Name = defaultBackground.Name,
                Description = defaultBackground.Description,
                Price = defaultBackground.Price,
                ProductType = defaultBackground.ProductTypeId
            };

            playerProfile.Character = defaultCharacter == null ? null : new Product
            {
                Id = defaultCharacter.Id,
                Name = defaultCharacter.Name,
                Description = defaultCharacter.Description,
                Price = defaultCharacter.Price,
                ProductType = defaultCharacter.ProductTypeId
            };

            return playerProfile;
        }

       

    }
}
