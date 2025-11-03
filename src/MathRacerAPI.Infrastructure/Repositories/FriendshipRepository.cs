using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MathRacerAPI.Infrastructure.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly MathiRacerDbContext _context;

        public FriendshipRepository(MathiRacerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public async Task<Friendship?> GetFriendshipAsync(int playerId1, int playerId2)
        {
            var entity = await _context.Friendships
                .Include(f => f.RequestStatus)
                .Include(f => f.Player1)
                .Include(f => f.Player2)
                .FirstOrDefaultAsync(f =>
                    (f.PlayerId1 == playerId1 && f.PlayerId2 == playerId2) ||
                    (f.PlayerId1 == playerId2 && f.PlayerId2 == playerId1));

            return entity == null ? null : MapToDomain(entity);
        }

        public async Task AddFriendshipAsync(Friendship friendship)
        {
            if (friendship == null) throw new ArgumentNullException(nameof(friendship));

            // Convertir dominio a entidad de EF
            var entity = new FriendshipEntity
            {
                Id = friendship.Id,
                PlayerId1 = friendship.PlayerId1,
                PlayerId2 = friendship.PlayerId2,
                RequestStatusId = friendship.RequestStatus.Id,
                Deleted = friendship.Deleted
            };

            _context.Friendships.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFriendshipAsync(Friendship friendship)
        {

            if (friendship == null) throw new ArgumentNullException(nameof(friendship));

            // Convertir dominio a entidad de EF
            var entity = await _context.Friendships.FindAsync(friendship.Id);
            if (entity == null)
                throw new InvalidOperationException("Friendship not found in the database.");

            entity.PlayerId1 = friendship.PlayerId1;
            entity.PlayerId2 = friendship.PlayerId2;
            entity.RequestStatusId = friendship.RequestStatus.Id;
            entity.Deleted = friendship.Deleted;

            _context.Friendships.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PlayerProfile>> GetFriendsAsync(int playerId)
        {
            var friends = await _context.Friendships
                 .Include(f => f.RequestStatus)
                 .Include(f => f.Player1)
                     .ThenInclude(p => p.PlayerProducts)
                         .ThenInclude(pp => pp.Product)
                 .Include(f => f.Player2)
                     .ThenInclude(p => p.PlayerProducts)
                         .ThenInclude(pp => pp.Product)
                 .Where(f => (f.PlayerId1 == playerId || f.PlayerId2 == playerId)
                             && f.RequestStatus.Name == "Aceptada"
                             && !f.Deleted) 
                 .ToListAsync();

            return friends.Select(f =>
            {
                var friendEntity = f.PlayerId1 == playerId ? f.Player2 : f.Player1;

                // Obtener productos activos y mapear character (productTypeId == 2)
                var activeProducts = friendEntity.PlayerProducts
                    .Where(pp => pp.IsActive)
                    .Select(pp => pp.Product)
                    .ToList();

                var charEntity = activeProducts.FirstOrDefault(p => p.ProductTypeId == 2);

                return new PlayerProfile
                {
                    Id = friendEntity.Id,
                    Name = friendEntity.Name,
                    Email = friendEntity.Email,
                    Uid = friendEntity.Uid,
                    LastLevelId = friendEntity.LastLevelId,
                    Points = friendEntity.Points,
                    Coins = friendEntity.Coins,
                    Character = charEntity == null ? null : new Product
                    {
                        Id = charEntity.Id,
                        Name = charEntity.Name,
                        Description = charEntity.Description,
                        Price = charEntity.Price,
                        ProductType = charEntity.ProductTypeId
                    }
                };
            });
        }

        public async Task<IEnumerable<PlayerProfile>> GetPendingFriendRequestsAsync(int playerId)
        {
            var entities = await _context.Friendships
                .Include(f => f.RequestStatus)
                .Include(f => f.Player1)
                    .ThenInclude(p => p.PlayerProducts)
                        .ThenInclude(pp => pp.Product)
                .Include(f => f.Player2)
                .Where(f => f.PlayerId2 == playerId 
                            && f.RequestStatus.Name == "Pendiente"
                            && !f.Deleted)
                .ToListAsync();
            
               return entities.Select(f =>

                {
                var friendEntity = f.PlayerId1 == playerId ? f.Player2 : f.Player1;

                // Obtener productos activos y mapear character (productTypeId == 2)
                var activeProducts = friendEntity.PlayerProducts
                    .Where(pp => pp.IsActive)
                    .Select(pp => pp.Product)
                    .ToList();

                var charEntity = activeProducts.FirstOrDefault(p => p.ProductTypeId == 2);

                return new PlayerProfile
                {
                    Id = friendEntity.Id,
                    Name = friendEntity.Name,
                    Email = friendEntity.Email,
                    Uid = friendEntity.Uid,
                    LastLevelId = friendEntity.LastLevelId,
                    Points = friendEntity.Points,
                    Coins = friendEntity.Coins,
                    Character = charEntity == null ? null : new Product
                    {
                        Id = charEntity.Id,
                        Name = charEntity.Name,
                        Description = charEntity.Description,
                        Price = charEntity.Price,
                        ProductType = charEntity.ProductTypeId
                    }
                };
            }); 

           
        }


        public async Task<RequestStatus> GetRequestStatusByNameAsync(string statusName)
        {
            var entity = await _context.RequestStatuses.FirstAsync(s => s.Name == statusName);

            return new RequestStatus
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };
        }


        private Friendship MapToDomain(FriendshipEntity entity)
        {
            return new Friendship
            {
                Id = entity.Id,
                PlayerId1 = entity.PlayerId1,
                PlayerId2 = entity.PlayerId2,
                RequestStatus = new RequestStatus
                {
                    Id = entity.RequestStatus.Id,
                    Name = entity.RequestStatus.Name,
                    Description = entity.RequestStatus.Description
                },
                Player1 = entity.Player1 == null ? null : new PlayerProfile
                {
                    Id = entity.Player1.Id,
                    Name = entity.Player1.Name,
                    Email = entity.Player1.Email,
                    Uid = entity.Player1.Uid,
                    LastLevelId = entity.Player1.LastLevelId,
                    Points = entity.Player1.Points,
                    Coins = entity.Player1.Coins
                },
                Player2 = entity.Player2 == null ? null : new PlayerProfile
                {
                    Id = entity.Player2.Id,
                    Name = entity.Player2.Name,
                    Email = entity.Player2.Email,
                    Uid = entity.Player2.Uid,
                    LastLevelId = entity.Player2.LastLevelId,
                    Points = entity.Player2.Points,
                    Coins = entity.Player2.Coins
                },
                Deleted = entity.Deleted
            };
        }
    }
}



