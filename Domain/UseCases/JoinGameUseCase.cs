using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

public class JoinGameUseCase
{
    private readonly IGameRepository _gameRepository;

    public JoinGameUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<Game?> ExecuteAsync(int gameId, string playerName)
    {
        var game = await _gameRepository.GetByIdAsync(gameId); //Busco la partida por id
        if (game == null || game.Players.Count >= 2 || game.Status != GameStatus.WaitingForPlayers) //Verifico que me pueda unir
            return null;

        var player = new Player { Name = playerName, Id = 2}; //Si me puedo unir, creo el jugador y lo agrego a la partida
        game.Players.Add(player);

        if (game.Players.Count == 2)
            game.Status = GameStatus.InProgress; //Cambio el estado de la partida si ya hay 2 jugadores

        await _gameRepository.UpdateAsync(game); 
        return game;
    }
}