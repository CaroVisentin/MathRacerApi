using MathRacerAPI.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar wildcards de los jugadores
/// </summary>
public interface IWildcardRepository
{
    /// <summary>
    /// Obtiene todos los wildcards disponibles para un jugador
    /// </summary>
    Task<List<PlayerWildcard>> GetPlayerWildcardsAsync(int playerId);
    
    /// <summary>
    /// Reduce la cantidad de un wildcard específico del jugador
    /// </summary>
    Task<bool> ConsumeWildcardAsync(int playerId, int wildcardId);
    
    /// <summary>
    /// Verifica si un jugador tiene cantidad disponible de un wildcard
    /// </summary>
    Task<bool> HasWildcardAvailableAsync(int playerId, int wildcardId);
}