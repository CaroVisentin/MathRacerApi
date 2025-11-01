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
    /// Consume una unidad de energía del jugador
    /// </summary>
    Task<bool> ConsumeEnergyAsync(int playerId);
    
    /// <summary>
    /// Verifica si el jugador tiene energía disponible
    /// </summary>
    Task<bool> HasEnergyAsync(int playerId);
}
