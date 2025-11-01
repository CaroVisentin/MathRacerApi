using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.UseCases;

public interface IGetPlayerRankingUseCase
{
    Task<(List<PlayerProfile> top10, int currentPlayerPosition)> ExecuteAsync(int playerId);
}