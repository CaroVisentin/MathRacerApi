using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar la energía de los jugadores
/// </summary>
public interface IEnergyRepository
{
    /// <summary>
    /// Obtiene la energía actual de un jugador
    /// </summary>
    Task<int> GetPlayerEnergyAsync(int playerId);
    
    /// <summary>
    /// Consume una unidad de energía del jugador preservando el progreso de recarga
    /// </summary>
    Task<bool> ConsumeEnergyAsync(int playerId);
    
    /// <summary>
    /// Verifica si el jugador tiene energía disponible
    /// </summary>
    Task<bool> HasEnergyAsync(int playerId);
    
    /// <summary>
    /// Obtiene los datos completos de energía (cantidad y última fecha de consumo)
    /// </summary>
    Task<(int Amount, DateTime LastConsumptionDate)?> GetEnergyDataAsync(int playerId);
    
    /// <summary>
    /// Actualiza la cantidad de energía y la fecha de última recarga sin consumir
    /// </summary>
    Task UpdateEnergyAsync(int playerId, int newAmount, DateTime lastRechargeDate);
}
