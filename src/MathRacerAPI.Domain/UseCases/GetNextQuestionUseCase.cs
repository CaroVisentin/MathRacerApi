using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Text.Json;

namespace MathRacerAPI.Domain.UseCases;

public class GetNextQuestionUseCase
{
    private readonly IGameRepository _gameRepository;

    public GetNextQuestionUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<NextQuestionResult> ExecuteAsync(int gameId, int playerId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);

        if (game == null) return new NextQuestionResult(); //Si no existe la partida, devuelvo null (checkear los nulls)

        var player = game.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return new NextQuestionResult();

        if (player.PenaltyUntil.HasValue && player.PenaltyUntil > DateTime.UtcNow) //Verifico si el jugador está penalizado
        {
            var secondsLeft = (player.PenaltyUntil.Value - DateTime.UtcNow).TotalSeconds; //Calculo los segundos que le quedan de penalización
            return new NextQuestionResult { PenaltySecondsLeft = Math.Ceiling(secondsLeft) }; //Redondeo hacia arriba y devuelvo
        }

        int nextIndex = player.IndexAnswered;
        if (nextIndex >= game.Questions.Count)
            return new NextQuestionResult(); //Si ya respondió todas las preguntas, devuelvo null

        return new NextQuestionResult { Question = game.Questions[nextIndex] };
    }
}