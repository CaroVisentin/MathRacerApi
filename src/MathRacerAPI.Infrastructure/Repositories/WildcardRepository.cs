using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
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
}