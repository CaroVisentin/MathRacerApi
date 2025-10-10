using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using System.Text.Json;

namespace MathRacerAPI.Domain.UseCases;

public class SubmitAnswerUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameLogicService _gameLogicService;

    public SubmitAnswerUseCase(IGameRepository gameRepository, IGameLogicService gameLogicService)
    {
        _gameRepository = gameRepository;
        _gameLogicService = gameLogicService;
    }

    public async Task<Game?> ExecuteAsync(int gameId, int playerId, int answer)
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

        // Verificar si el jugador puede responder (no está en penalización)
        if (!_gameLogicService.CanPlayerAnswer(player))
            return game;

        int currentIndex = player.IndexAnswered;    //Determino el índice de la pregunta actual
        if (currentIndex >= game.Questions.Count)   //Significa que el juego ya terminó para ese jugador
            return game; 

        var question = game.Questions[currentIndex];
        bool isCorrect = question.CorrectAnswer == answer;

        // Aplicar resultado de la respuesta usando el servicio de dominio
        _gameLogicService.ApplyAnswerResult(player, isCorrect);
       
        // Actualizar posiciones de jugadores
        _gameLogicService.UpdatePlayerPositions(game);

        // Verificar condiciones de finalización usando el servicio de dominio
        _gameLogicService.CheckAndUpdateGameEndConditions(game);

        await _gameRepository.UpdateAsync(game);
        return game;
    }
}