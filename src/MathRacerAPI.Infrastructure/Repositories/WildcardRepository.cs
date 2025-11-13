using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories;

public class WildcardRepository : IWildcardRepository
{
    private readonly MathiRacerDbContext _context;

    public WildcardRepository(MathiRacerDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlayerWildcard>> GetPlayerWildcardsAsync(int playerId)
    {
        var entities = await _context.PlayerWildcards
            .Include(pw => pw.Wildcard)
            .Where(pw => pw.PlayerId == playerId && pw.Quantity > 0)
            .ToListAsync();

        return entities.Select(e => new PlayerWildcard
        {
            Id = e.Id,
            PlayerId = e.PlayerId,
            WildcardId = e.WildcardId,
            Quantity = e.Quantity,
            Wildcard = new Wildcard
            {
                Id = e.Wildcard.Id,
                Name = e.Wildcard.Name,
                Description = e.Wildcard.Description,
            }
        }).ToList();
    }

    public async Task<bool> ConsumeWildcardAsync(int playerId, int wildcardId)
    {
        var entity = await _context.PlayerWildcards
            .FirstOrDefaultAsync(pw => pw.PlayerId == playerId && pw.WildcardId == wildcardId);

        if (entity == null || entity.Quantity <= 0)
        {
            return false;
        }

        entity.Quantity--;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasWildcardAvailableAsync(int playerId, int wildcardId)
    {
        var entity = await _context.PlayerWildcards
            .FirstOrDefaultAsync(pw => pw.PlayerId == playerId && pw.WildcardId == wildcardId);

        return entity != null && entity.Quantity > 0;
    }

    public async Task<List<Wildcard>> GetStoreWildcardsAsync()
    {
        var entities = await _context.Wildcards
            .Where(w => w.Price > 0) // Solo wildcards que se pueden comprar
            .OrderBy(w => w.Name)
            .ToListAsync();

        return entities.Select(e => new Wildcard
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            Price = e.Price
        }).ToList();
    }

    public async Task<Wildcard?> GetWildcardByIdAsync(int wildcardId)
    {
        var entity = await _context.Wildcards
            .FirstOrDefaultAsync(w => w.Id == wildcardId);

        if (entity == null)
            return null;

        return new Wildcard
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price
        };
    }

    public async Task<bool> PurchaseWildcardAsync(int playerId, int wildcardId, int quantity, int totalPrice)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Descontar las monedas del jugador
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null || player.Coins < totalPrice)
            {
                await transaction.RollbackAsync();
                return false;
            }

            player.Coins -= totalPrice;

            // Buscar o crear el registro de wildcard del jugador
            var playerWildcard = await _context.PlayerWildcards
                .FirstOrDefaultAsync(pw => pw.PlayerId == playerId && pw.WildcardId == wildcardId);

            if (playerWildcard == null)
            {
                // Crear nuevo registro
                playerWildcard = new PlayerWildcardEntity
                {
                    PlayerId = playerId,
                    WildcardId = wildcardId,
                    Quantity = quantity
                };
                _context.PlayerWildcards.Add(playerWildcard);
            }
            else
            {
                // Actualizar cantidad existente
                playerWildcard.Quantity += quantity;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}