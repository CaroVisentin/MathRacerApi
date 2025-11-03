using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para otorgar recompensas al completar un nivel
/// </summary>
public class GrantLevelRewardUseCase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly Random _random = new();

    public GrantLevelRewardUseCase(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Otorga recompensas al jugador por completar un nivel
    /// </summary>
    /// <param name="playerId">ID del jugador</param>
    /// <param name="levelId">ID del nivel completado</param>
    /// <param name="worldId">ID del mundo al que pertenece el nivel</param>
    /// <returns>Cantidad de monedas otorgadas</returns>
    public async Task<int> ExecuteAsync(int playerId, int levelId, int worldId)
    {
        // Obtener el jugador para verificar si es primera vez
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
            throw new NotFoundException("No se encontró un jugador con el UID proporcionado.");

        // Determinar si es la primera vez que completa este nivel
        // Un nivel ya fue completado si el LastLevelId del jugador es > al levelId actual
        bool isFirstCompletion = levelId > player.LastLevelId;

        // Calcular recompensa
        int coins = CalculateReward(worldId, isFirstCompletion);

        // Otorgar monedas
        await _playerRepository.AddCoinsAsync(playerId, coins);

        // Actualizar último nivel completado si corresponde
        if (isFirstCompletion)
        {
            await _playerRepository.UpdateLastLevelAsync(playerId, levelId);
        }

        return coins;
    }

    /// <summary>
    /// Calcula la cantidad de monedas a otorgar
    /// </summary>
    private int CalculateReward(int worldId, bool isFirstCompletion)
    {
        int baseReward = worldId * 100;

        if (isFirstCompletion)
        {
            // Primera vez: ±20%
            double variation = _random.NextDouble() * 0.4 - 0.2; // Rango [-0.2, 0.2]
            int coins = (int)(baseReward * (1 + variation));
            return Math.Max(coins, 1);
        }
        else
        {
            // Repetición: 10% ±1%
            int reducedBase = baseReward / 10;
            double variation = _random.NextDouble() * 0.02 - 0.01; // Rango [-0.01, 0.01]
            int coins = (int)(reducedBase * (1 + variation));
            return Math.Max(coins, 1);
        }
    }
}