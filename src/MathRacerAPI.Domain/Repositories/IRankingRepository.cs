using MathRacerAPI.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories;

public interface IRankingRepository
{
    /// <summary>
    /// Obtiene el top 10 de jugadores y la posición del jugador actual
    /// </summary>
    /// <param name="playerId">Id del jugador actual</param>
    /// <returns>Lista top 10 y posición del jugador</returns>
    Task<(List<PlayerProfile> top10, int currentPlayerPosition)> GetTop10WithPlayerPositionAsync(int playerId);
}
