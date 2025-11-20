using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener la lista de partidas disponibles
/// </summary>
public class GetAvailableGamesUseCase
{
    private readonly IGameRepository _gameRepository;

    public GetAvailableGamesUseCase(
        IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    /// <summary>
    /// Obtiene todas las partidas disponibles (públicas y privadas en estado WaitingForPlayers)
    /// Excluye partidas creadas por invitación entre amigos
    /// </summary>
    /// <param name="includePrivate">Si se deben incluir partidas privadas en la lista</param>
    /// <returns>Lista de partidas disponibles</returns>
    public async Task<List<AvailableGameInfo>> ExecuteAsync(bool includePrivate = true)
    {
        // Obtener todas las partidas
        var allGames = await _gameRepository.GetAllAsync();
        if (allGames == null)
            throw new NullReferenceException("La lista de partidas no puede ser nula.");

        // Filtrar partidas disponibles
        var availableGames = allGames
            .Where(g => 
                g.Status == GameStatus.WaitingForPlayers && // Solo partidas esperando jugadores
                g.Players.Count < 2 && // No llenas
                g.Id >= 1000 && // Solo partidas online (filtro por ID)
                !g.IsFromInvitation) // Excluir partidas por invitación
            .Where(g => includePrivate || !g.IsPrivate) // Filtrar privadas si se requiere
            .OrderByDescending(g => g.CreatedAt) // Más recientes primero
            .Select(AvailableGameInfo.FromGame)
            .ToList();

        return availableGames;
    }

    /// <summary>
    /// Obtiene solo las partidas públicas disponibles
    /// </summary>
    public async Task<List<AvailableGameInfo>> GetPublicGamesAsync()
    {
        return await ExecuteAsync(includePrivate: false);
    }
}