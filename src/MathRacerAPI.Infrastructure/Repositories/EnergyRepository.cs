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

    public async Task<(int Price, int MaxAmount)?> GetEnergyConfigurationAsync()
    {
        var config = await _context.EnergyConfigurations.FirstOrDefaultAsync();
        
        if (config == null)
            return null;
            
        return (config.Price, config.MaxAmount);
    }

    public async Task<bool> PurchaseEnergyAsync(int playerId, int quantity, int totalPrice)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Obtener el jugador
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            if (player == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Verificar que tiene suficientes monedas
            if (player.Coins < (decimal)totalPrice)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Obtener energía actual
            var energy = await _context.Energies.FirstOrDefaultAsync(e => e.PlayerId == playerId);
            if (energy == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Actualizar monedas del jugador
            player.Coins -= totalPrice;

            // Actualizar energía
            energy.Amount += quantity;
            
            // Resetear la fecha para que no se considere tiempo de recarga pendiente
            energy.LastConsumptionDate = DateTime.UtcNow;

            // Guardar cambios
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
