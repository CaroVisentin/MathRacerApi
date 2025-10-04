using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

public class SubmitAnswerUseCase
{
    private readonly IGameRepository _gameRepository;

    public SubmitAnswerUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<Game?> ExecuteAsync(int gameId, int playerId, string answer)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            return null;
        }

        if (game.Status == GameStatus.Finished) //Si la partida ya terminó, no puedo seguir respondiendo
            return game;

        var player = game.Players.FirstOrDefault(p => p.Id == playerId); //Encuentro el jugador que responde
        if (player == null) return null;

        int currentIndex = player.IndexAnswered;    //Determino el índice de la pregunta actual
        if (currentIndex >= game.Questions.Count)   //Significa que el juego ya terminó para ese jugador
            return game; 

        var question = game.Questions[currentIndex];

        if (question.CorrectAnswer == answer) //Respuesta correcta
        {
            player.CorrectAnswers++;
            player.Position ++; 
            player.PenaltyUntil = null; //Elimino la penalización si la tenía

        }
        else
        {
            player.PenaltyUntil = DateTime.UtcNow.AddSeconds(2); //Penalizo al jugador por 2 segundos
        }

        player.IndexAnswered++;

        //Finalizo la partida si el jugador contestó correctamente la cantidad de preguntas necesarias 
        if (player.CorrectAnswers >= game.ConditionToWin)
        {
            if (player.FinishedAt == null)
                player.FinishedAt = DateTime.UtcNow;

            game.Status = GameStatus.Finished;
            game.WinnerId = player.Id;
        }
        else if (player.IndexAnswered >= game.Questions.Count) //Finalizo si el jugador respondió todas las preguntas, todavía no ganó, espero al desempate
        {
            if (player.FinishedAt == null)
                player.FinishedAt = DateTime.UtcNow;
        }
        //Finalizo si todos respondieron todas las preguntas
        else if (game.Players.All(p => p.IndexAnswered >= game.Questions.Count))
        {
            var maxCorrect = game.Players.Max(p => p.CorrectAnswers); //Obtengo la mayor cantidad de respuestas correctas
            var winners = game.Players.Where(p => p.CorrectAnswers == maxCorrect).ToList(); //Obtengo los jugadores que tienen esa cantidad de respuestas correctas

            if (winners.Count == 1)     //Si hay un solo ganador, lo asigno
            {
                game.WinnerId = winners[0].Id;
            }
            else //Si hay más de un ganador, hago el desempate
            {
                var minFinished = winners.Min(p => p.FinishedAt ?? DateTime.MaxValue);
                var winner = winners.First(p => p.FinishedAt == minFinished);
                game.WinnerId = winner.Id;
            }
            game.Status = GameStatus.Finished;
        }

        await _gameRepository.UpdateAsync(game);
        return game;
    }
}