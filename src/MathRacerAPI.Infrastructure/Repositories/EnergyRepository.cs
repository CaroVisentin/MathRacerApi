using MathRacerAPI.Domain.Constants;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

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
        
        var now = DateTime.UtcNow;
        var timeSinceLastConsumption = now - energy.LastConsumptionDate;
        var secondsPassed = (int)timeSinceLastConsumption.TotalSeconds;
        
        // Calcular progreso en el ciclo actual (segundos que NO cuentan como recarga completa)
        int progressSeconds = secondsPassed % EnergyConstants.SECONDS_PER_RECHARGE;
        
        // Decrementar energía
        energy.Amount--;
        
        // Ajustar LastConsumptionDate para preservar el progreso
        energy.LastConsumptionDate = now.AddSeconds(-progressSeconds);
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(int Amount, DateTime LastConsumptionDate)?> GetEnergyDataAsync(int playerId)
    {
        var energy = await _context.Energies
            .FirstOrDefaultAsync(e => e.PlayerId == playerId);
            
        if (energy == null)
            return null;
            
        return (energy.Amount, energy.LastConsumptionDate);
    }

    public async Task UpdateEnergyAsync(int playerId, int newAmount, DateTime lastRechargeDate)
    {
        var energy = await _context.Energies
            .FirstOrDefaultAsync(e => e.PlayerId == playerId);
            
        if (energy == null)
            return;
            
        energy.Amount = newAmount;
        energy.LastConsumptionDate = lastRechargeDate;
        
        await _context.SaveChangesAsync();
    }
}
