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
    public class GarageRepository : IGarageRepository
    {
        private readonly MathiRacerDbContext _context;
        private readonly IPlayerRepository _playerRepository;

        public GarageRepository(MathiRacerDbContext context, IPlayerRepository playerRepository)
        {
            _context = context;
            _playerRepository = playerRepository;
        }

        public async Task<GarageItemsResponse> GetPlayerItemsByTypeAsync(int playerId, string productType)
        {
            // Validate that the player exists
            var player = await _playerRepository.GetByIdAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} does not exist", nameof(playerId));

            var allItemsOfType = await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.Rarity)
                .Where(p => p.ProductType.Name == productType)
                .Select(p => new GarageItem
                {
                    Id = p.Id,
                    ProductId = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = (decimal)p.Price,
                    ProductType = p.ProductType.Name,
                    Rarity = p.Rarity != null ? p.Rarity.Rarity : "Common",
                    IsOwned = false,
                    IsActive = false
                })
                .ToListAsync();

            var ownedItems = await _context.PlayerProducts
                .Include(pp => pp.Product)
                .ThenInclude(p => p.ProductType)
                .Include(pp => pp.Product)
                .ThenInclude(p => p.Rarity)
                .Where(pp => pp.PlayerId == playerId && pp.Product.ProductType.Name == productType)
                .ToListAsync();

            GarageItem? activeItem = null;

            // Update owned items and find active item
            foreach (var ownedItem in ownedItems)
            {
                var item = allItemsOfType.FirstOrDefault(i => i.ProductId == ownedItem.ProductId);
                if (item != null)
                {
                    item.IsOwned = true;
                    item.IsActive = ownedItem.IsActive;
                    
                    if (ownedItem.IsActive)
                    {
                        activeItem = item;
                    }
                }
            }

            return new GarageItemsResponse
            {
                Items = allItemsOfType,
                ActiveItem = activeItem,
                ItemType = productType
            };
        }

        public async Task<bool> ActivatePlayerItemAsync(int playerId, int productId, string productType)
        {
            // Validate that the player exists
            var player = await _playerRepository.GetByIdAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} does not exist", nameof(playerId));

            // First, verify the player owns this item
            var playerProduct = await _context.PlayerProducts
                .Include(pp => pp.Product)
                .ThenInclude(p => p.ProductType)
                .FirstOrDefaultAsync(pp => pp.PlayerId == playerId && 
                                         pp.ProductId == productId && 
                                         pp.Product.ProductType.Name == productType);

            if (playerProduct == null)
                return false;

            // Check if the requested item is already active - no need to do anything
            if (playerProduct.IsActive)
                return true;

            // Find the currently active item of this type (should be only one)
            var currentActiveItem = await _context.PlayerProducts
                .Include(pp => pp.Product)
                .ThenInclude(p => p.ProductType)
                .FirstOrDefaultAsync(pp => pp.PlayerId == playerId && 
                                         pp.Product.ProductType.Name == productType && 
                                         pp.IsActive);

            // Deactivate the current active item (if any)
            if (currentActiveItem != null)
            {
                currentActiveItem.IsActive = false;
            }

            // Activate the requested item
            playerProduct.IsActive = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GarageItem?> GetActiveItemByTypeAsync(int playerId, string productType)
        {
            // Validate that the player exists
            var player = await _playerRepository.GetByIdAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} does not exist", nameof(playerId));

            var activePlayerProduct = await _context.PlayerProducts
                .Include(pp => pp.Product)
                .ThenInclude(p => p.ProductType)
                .Include(pp => pp.Product)
                .ThenInclude(p => p.Rarity)
                .FirstOrDefaultAsync(pp => pp.PlayerId == playerId && 
                                         pp.Product.ProductType.Name == productType && 
                                         pp.IsActive);

            if (activePlayerProduct == null)
                return null;

            return new GarageItem
            {
                Id = activePlayerProduct.Id,
                ProductId = activePlayerProduct.ProductId,
                Name = activePlayerProduct.Product.Name,
                Description = activePlayerProduct.Product.Description,
                Price = (decimal)activePlayerProduct.Product.Price,
                ProductType = activePlayerProduct.Product.ProductType.Name,
                Rarity = activePlayerProduct.Product.Rarity?.Rarity ?? "Common",
                IsOwned = true,
                IsActive = true
            };
        }
    }
}