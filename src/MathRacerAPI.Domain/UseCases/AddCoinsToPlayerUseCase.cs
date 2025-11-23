using System.Threading.Tasks;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

public class AddCoinsToPlayerUseCase
{
    private readonly IPlayerRepository _playerRepository;

    public AddCoinsToPlayerUseCase(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task ExecuteAsync(int playerId, int coinAmount)
    {
        if (coinAmount <= 0)
        {
            return;
        }

        await _playerRepository.AddCoinsAsync(playerId, coinAmount);
    }
}
