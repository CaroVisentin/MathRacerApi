using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories;

public class EnergyRepository : IEnergyRepository
{
    private readonly MathiRacerDbContext _context;

    public EnergyRepository(MathiRacerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> GetPlayerEnergyAsync(int playerId)
    {
        var energy = await _context.Energies
            .FirstOrDefaultAsync(e => e.PlayerId == playerId);
            
        return energy?.Amount ?? 0;
    }

    public async Task<bool> HasEnergyAsync(int playerId)
    {
        var energyAmount = await GetPlayerEnergyAsync(playerId);
        return energyAmount > 0;
    }

    public async Task<bool> ConsumeEnergyAsync(int playerId)
    {
        var energy = await _context.Energies
            .FirstOrDefaultAsync(e => e.PlayerId == playerId);
            
        if (energy == null || energy.Amount <= 0)
            return false;
            
        energy.Amount--;
        energy.LastConsumptionDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
