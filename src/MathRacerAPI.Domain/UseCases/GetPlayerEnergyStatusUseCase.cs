using MathRacerAPI.Domain.Constants;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener el estado actual de energía de un jugador
/// </summary>
public class GetPlayerEnergyStatusUseCase
{
    private readonly IEnergyRepository _energyRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerEnergyStatusUseCase(
        IEnergyRepository energyRepository,
        IPlayerRepository playerRepository)
    {
        _energyRepository = energyRepository ?? throw new ArgumentNullException(nameof(energyRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    /// <summary>
    /// Calcula el estado actual de energía basándose en el UID del jugador
    /// </summary>
    public async Task<EnergyStatus> ExecuteByUidAsync(string uid)
    {
        var player = await _playerRepository.GetByUidAsync(uid);
        
        if (player == null)
        {
            throw new ArgumentException($"No se encontró un jugador con UID: {uid}");
        }

        return await ExecuteAsync(player.Id);
    }

    /// <summary>
    /// Calcula el estado actual de energía basándose en el tiempo transcurrido y persiste los cambios
    /// </summary>
    public async Task<EnergyStatus> ExecuteAsync(int playerId)
    {
        var energyData = await _energyRepository.GetEnergyDataAsync(playerId);
        
        if (energyData == null)
        {
            return new EnergyStatus
            {
                CurrentAmount = EnergyConstants.MAX_ENERGY,
                MaxAmount = EnergyConstants.MAX_ENERGY,
                SecondsUntilNextRecharge = null,
                LastCalculatedRecharge = DateTime.UtcNow
            };
        }

        var (currentAmount, lastConsumptionDate) = energyData.Value;
        var energyStatus = CalculateEnergyStatus(currentAmount, lastConsumptionDate);

        // Si la energía ha cambiado (se recargó), persistir en BD
        if (energyStatus.CurrentAmount > currentAmount)
        {
            var rechargesCompleted = energyStatus.CurrentAmount - currentAmount;
            var adjustedLastConsumptionDate = lastConsumptionDate.AddSeconds(rechargesCompleted * EnergyConstants.SECONDS_PER_RECHARGE);
            
            await _energyRepository.UpdateEnergyAsync(
                playerId, 
                energyStatus.CurrentAmount, 
                adjustedLastConsumptionDate
            );
        }

        return energyStatus;
    }

    /// <summary>
    /// Calcula el estado de energía basado en la cantidad actual y última fecha de consumo
    /// </summary>
    private EnergyStatus CalculateEnergyStatus(int currentAmount, DateTime lastConsumptionDate)
    {
        if (currentAmount >= EnergyConstants.MAX_ENERGY)
        {
            return new EnergyStatus
            {
                CurrentAmount = EnergyConstants.MAX_ENERGY,
                MaxAmount = EnergyConstants.MAX_ENERGY,
                SecondsUntilNextRecharge = null,
                LastCalculatedRecharge = DateTime.UtcNow
            };
        }

        var now = DateTime.UtcNow;
        var timeSinceLastConsumption = now - lastConsumptionDate;
        var secondsPassed = (int)timeSinceLastConsumption.TotalSeconds;

        // Calcular cuánta energía se ha recargado
        int rechargedEnergy = secondsPassed / EnergyConstants.SECONDS_PER_RECHARGE;
        int newAmount = Math.Min(currentAmount + rechargedEnergy, EnergyConstants.MAX_ENERGY);

        // Calcular segundos hasta la próxima recarga
        int? secondsUntilNext = null;
        if (newAmount < EnergyConstants.MAX_ENERGY)
        {
            int secondsIntoCurrentCycle = secondsPassed % EnergyConstants.SECONDS_PER_RECHARGE;
            secondsUntilNext = EnergyConstants.SECONDS_PER_RECHARGE - secondsIntoCurrentCycle;
        }

        return new EnergyStatus
        {
            CurrentAmount = newAmount,
            MaxAmount = EnergyConstants.MAX_ENERGY,
            SecondsUntilNextRecharge = secondsUntilNext,
            LastCalculatedRecharge = now
        };
    }
}