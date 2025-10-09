using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener la siguiente pregunta en una partida online
/// </summary>
public class GetNextOnlineQuestionUseCase
{
    private readonly IGameRepository _gameRepository;

    public GetNextOnlineQuestionUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<Question?> ExecuteAsync(int gameId, int playerId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null) return null;

        var player = game.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return null;

        int nextIndex = player.IndexAnswered;
        if (nextIndex >= game.Questions.Count) return null;

        return game.Questions[nextIndex];
    }
}