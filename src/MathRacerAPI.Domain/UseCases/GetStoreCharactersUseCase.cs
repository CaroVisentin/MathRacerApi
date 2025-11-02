using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener todos los personajes disponibles en la tienda
/// </summary>
public class GetStoreCharactersUseCase
{
    private readonly IStoreRepository _storeRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetStoreCharactersUseCase(IStoreRepository storeRepository, IPlayerRepository playerRepository)
    {
        _storeRepository = storeRepository;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Obtiene todos los personajes disponibles en la tienda para un jugador específico
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <returns>Lista de personajes con información de propiedad</returns>
    /// <exception cref="NotFoundException">Se lanza cuando el jugador no existe</exception>
    public async Task<List<StoreItem>> ExecuteAsync(int playerId)
    {
        // Verificar que el jugador existe
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException("Jugador no encontrado");
        }

        // Obtener personajes (ProductTypeId = 2)
        return await _storeRepository.GetProductsByTypeAsync(2, playerId);
    }
}