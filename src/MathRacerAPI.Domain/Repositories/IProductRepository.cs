using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Repositories;

/// <summary>
/// Interfaz para acceder a productos de jugadores
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Obtiene los productos activos de un jugador (auto, personaje, fondo)
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <returns>Lista de productos activos</returns>
    Task<List<PlayerProduct>> GetActiveProductsByPlayerIdAsync(int playerId);

    /// <summary>
    /// Obtiene productos aleatorios para la máquina (1 auto, 1 personaje, 1 fondo)
    /// </summary>
    /// <returns>Lista de 3 productos aleatorios</returns>
    Task<List<PlayerProduct>> GetRandomProductsForMachineAsync();
}
