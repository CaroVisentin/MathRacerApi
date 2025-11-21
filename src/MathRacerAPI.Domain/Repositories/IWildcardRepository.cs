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
    
    /// <summary>
    /// Obtiene todos los wildcards disponibles en la tienda
    /// </summary>
    Task<List<Wildcard>> GetStoreWildcardsAsync();
    
    /// <summary>
    /// Obtiene un wildcard por su ID
    /// </summary>
    Task<Wildcard?> GetWildcardByIdAsync(int wildcardId);
    
    /// <summary>
    /// Procesa la compra de wildcards para un jugador
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <param name="wildcardId">ID del wildcard</param>
    /// <param name="quantity">Cantidad a comprar</param>
    /// <param name="totalPrice">Precio total de la compra</param>
    Task<bool> PurchaseWildcardAsync(int playerId, int wildcardId, int quantity, int totalPrice);
}