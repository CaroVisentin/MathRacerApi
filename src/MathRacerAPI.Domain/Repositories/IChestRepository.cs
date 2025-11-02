using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Repositories;

/// <summary>
/// Repositorio para operaciones de cofres
/// </summary>
public interface IChestRepository
{
    /// <summary>
    /// Obtiene 3 productos comunes aleatorios (1 de cada tipo)
    /// Para el cofre tutorial inicial
    /// </summary>
    Task<List<Product>> GetTutorialProductsAsync();

    /// <summary>
    /// Obtiene un producto aleatorio basado en probabilidades de rareza
    /// Usa RarityEntity.Probability de la BD
    /// </summary>
    Task<Product?> GetRandomProductByRarityProbabilityAsync();

    /// <summary>
    /// Obtiene un wildcard aleatorio completo desde la base de datos
    /// </summary>
    Task<Wildcard?> GetRandomWildcardAsync();

    /// <summary>
    /// Verifica si el jugador ya posee un producto
    /// </summary>
    Task<bool> PlayerHasProductAsync(int playerId, int productId);

    /// <summary>
    /// Asigna productos al jugador
    /// </summary>
    Task AssignProductsToPlayerAsync(int playerId, List<int> productIds, bool setAsActive);

    /// <summary>
    /// Incrementa las monedas del jugador
    /// </summary>
    Task AddCoinsToPlayerAsync(int playerId, int coins);

    /// <summary>
    /// Incrementa los wildcards del jugador por tipo
    /// </summary>
    Task AddWildcardsToPlayerAsync(int playerId, int wildcardId, int quantity);

    /// <summary>
    /// Obtiene los productos activos del jugador
    /// Usado para verificar si ya completó el tutorial
    /// </summary>
    Task<List<PlayerProduct>> GetActiveProductsByPlayerIdAsync(int playerId);
}