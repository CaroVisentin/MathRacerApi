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

            if (entity == null) return null;

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
                }
           
            };
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
                            && f.RequestStatus.Name == "Aceptada")
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

        public async Task SendFriendRequestAsync(int fromPlayerId, int toPlayerId)
        {
            var status = await _context.RequestStatuses.FirstAsync(s => s.Name == "Pendiente");

            _context.Friendships.Add(new FriendshipEntity
            {
                PlayerId1 = fromPlayerId,
                PlayerId2 = toPlayerId,
                RequestStatusId = status.Id
            });

            await _context.SaveChangesAsync();
        }

        public async Task AcceptFriendRequestAsync(int fromPlayerId, int toPlayerId)
        {
            var entity = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.PlayerId1 == fromPlayerId && f.PlayerId2 == toPlayerId) ||
                    (f.PlayerId1 == toPlayerId && f.PlayerId2 == fromPlayerId));

            if (entity == null) return;

            var status = await _context.RequestStatuses.FirstAsync(s => s.Name == "Aceptada");
            entity.RequestStatusId = status.Id;

            await _context.SaveChangesAsync();
        }

        public async Task RejectFriendRequestAsync(int fromPlayerId, int toPlayerId)
        {
            var entity = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.PlayerId1 == fromPlayerId && f.PlayerId2 == toPlayerId) ||
                    (f.PlayerId1 == toPlayerId && f.PlayerId2 == fromPlayerId));

            if (entity == null) return;

            var status = await _context.RequestStatuses.FirstAsync(s => s.Name == "Rechazada");
            entity.RequestStatusId = status.Id;

            await _context.SaveChangesAsync();
        }
    }

}
