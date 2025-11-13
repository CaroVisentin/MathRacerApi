using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener información de wildcards disponibles en la tienda
/// </summary>
public class GetStoreWildcardsUseCase
{
    private readonly IWildcardRepository _wildcardRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetStoreWildcardsUseCase(IWildcardRepository wildcardRepository, IPlayerRepository playerRepository)
    {
        _wildcardRepository = wildcardRepository;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Obtiene la lista de wildcards disponibles para compra con información del jugador
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <returns>Lista de wildcards con precios y cantidades actuales</returns>
    /// <exception cref="NotFoundException">Se lanza cuando el jugador no existe</exception>
    public async Task<List<StoreWildcard>> ExecuteAsync(int playerId)
    {
        // Verificar que el jugador existe
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException("Jugador no encontrado");
        }

        // Obtener todos los wildcards disponibles en la tienda
        var storeWildcards = await _wildcardRepository.GetStoreWildcardsAsync();
        
        // Obtener las cantidades actuales del jugador
        var playerWildcards = await _wildcardRepository.GetPlayerWildcardsAsync(playerId);

        // Combinar información de tienda con cantidades del jugador
        var result = storeWildcards.Select(wildcard => new StoreWildcard
        {
            Id = wildcard.Id,
            Name = wildcard.Name,
            Description = wildcard.Description,
            Price = (int)wildcard.Price,
            CurrentQuantity = playerWildcards.FirstOrDefault(pw => pw.WildcardId == wildcard.Id)?.Quantity ?? 0
        }).ToList();

        return result;
    }
}