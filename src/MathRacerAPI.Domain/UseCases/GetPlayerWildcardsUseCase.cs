using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener los wildcards de un jugador
/// </summary>
public class GetPlayerWildcardsUseCase
{
    private readonly IWildcardRepository _wildcardRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerWildcardsUseCase(
        IWildcardRepository wildcardRepository,
        IPlayerRepository playerRepository)
    {
        _wildcardRepository = wildcardRepository ?? throw new ArgumentNullException(nameof(wildcardRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    /// <summary>
    /// Obtiene los wildcards del jugador por su UID de Firebase
    /// </summary>
    /// <param name="uid">UID de Firebase del jugador</param>
    /// <returns>Lista de wildcards del jugador</returns>
    public async Task<List<PlayerWildcard>> ExecuteByUidAsync(string uid)
    {
        var player = await _playerRepository.GetByUidAsync(uid);
        
        if (player == null)
        {
            throw new InvalidOperationException($"Jugador con UID '{uid}' no encontrado.");
        }

        var wildcards = await _wildcardRepository.GetPlayerWildcardsAsync(player.Id);
        return wildcards.OrderBy(w => w.WildcardId).ToList();
    }
}