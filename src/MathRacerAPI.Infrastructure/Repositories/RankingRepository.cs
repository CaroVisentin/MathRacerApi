using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories;

public class RankingRepository : IRankingRepository
{
    private readonly MathiRacerDbContext _context;

    public RankingRepository(MathiRacerDbContext context)
    {
        _context = context;
    }

    public async Task<(List<PlayerProfile> top10, int currentPlayerPosition)> GetTop10WithPlayerPositionAsync(int playerId)
    {
        var entities = await _context.Players
            .Where(p => !p.Deleted)
            .OrderByDescending(p => p.Points)
            .ToListAsync();
        var profiles = entities.Select(e => new PlayerProfile
        {
            Id = e.Id,
            Name = e.Name,
            Points = e.Points
        }).ToList();
        var top10 = profiles.Take(10).ToList();
        int position = profiles.FindIndex(p => p.Id == playerId);
        return (top10, position != -1 ? position + 1 : 0);
    }
}
