using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using static MathRacerAPI.Domain.Models.ChestItem;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para abrir el cofre tutorial al registrar un jugador
/// Asigna 3 productos comunes como activos
/// </summary>
public class OpenTutorialChestUseCase
{
    private readonly IChestRepository _chestRepository;
    private readonly IPlayerRepository _playerRepository;

    public OpenTutorialChestUseCase(
        IChestRepository chestRepository,
        IPlayerRepository playerRepository)
    {
        _chestRepository = chestRepository;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Ejecuta la apertura del cofre tutorial usando el UID del jugador
    /// </summary>
    /// <param name="playerUid">UID de Firebase del jugador</param>
    /// <returns>Cofre con los productos iniciales</returns>
    public async Task<Chest> ExecuteAsync(string playerUid)
    {
        // 1. Obtener playerId desde el UID
        var playerProfile = await _playerRepository.GetByUidAsync(playerUid);

        if (playerProfile == null)
        {
            throw new NotFoundException($"Jugador con UID '{playerUid}' no encontrado.");
        }

        // 2. Verificar si ya tiene productos activos (significa que ya abrió el cofre)
        var existingProducts = await _chestRepository.GetActiveProductsByPlayerIdAsync(playerProfile.Id);
        if (existingProducts != null && existingProducts.Count >= 3)
        {
            throw new BusinessException("Ya has completado el tutorial y recibido tus productos iniciales.");
        }

        // 3. Obtener 3 productos comunes (1 auto, 1 personaje, 1 fondo)
        var tutorialProducts = await _chestRepository.GetTutorialProductsAsync();

        // 4. Crear estructura de cofre para frontend
        var chest = new Chest
        {
            Items = tutorialProducts.Select(p => new ChestItem
            {
                Type = ChestItemType.Product,
                Quantity = 1,
                Product = p
            }).ToList()
        };

        // 4. Asignar productos como activos al jugador (actualiza BD)
        var productIds = tutorialProducts.Select(p => p.Id).ToList();
        await _chestRepository.AssignProductsToPlayerAsync(playerProfile.Id, productIds, setAsActive: true);

        return chest;
    }
}