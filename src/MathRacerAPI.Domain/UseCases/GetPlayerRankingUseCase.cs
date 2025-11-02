using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases;

public class GetPlayerRankingUseCase : IGetPlayerRankingUseCase
{
    private readonly IRankingRepository _rankingRepository;

    public GetPlayerRankingUseCase(IRankingRepository rankingRepository)
    {
        _rankingRepository = rankingRepository;
    }

    public async Task<(List<PlayerProfile> top10, int currentPlayerPosition)> ExecuteAsync(int playerId)
    {
        return await _rankingRepository.GetTop10WithPlayerPositionAsync(playerId);
    }
}
