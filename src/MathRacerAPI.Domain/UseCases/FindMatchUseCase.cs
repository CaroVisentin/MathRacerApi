using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Providers;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para encontrar o crear una partida multijugador online
/// </summary>
public class FindMatchUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IQuestionProvider _questionProvider;
    private readonly IGameLogicService _gameLogicService;
    private static int _nextPlayerId = 1000; // Empezar desde 1000 para diferenciar del modo offline
    private static int _nextGameId = 1000;

    public FindMatchUseCase(
        IGameRepository gameRepository, 
        IQuestionProvider questionProvider,
        IGameLogicService gameLogicService)
    {
        _gameRepository = gameRepository;
        _questionProvider = questionProvider;
        _gameLogicService = gameLogicService;
    }

    public async Task<Game> ExecuteAsync(string playerName, string connectionId)
    {
        // Crear jugador con ID único
        var player = new Player 
        { 
            Id = Interlocked.Increment(ref _nextPlayerId),
            Name = playerName,
            ConnectionId = connectionId
        };

        // Buscar una partida esperando jugadores
        var availableGames = await _gameRepository.GetAllAsync();
        var waitingGame = availableGames.FirstOrDefault(g => 
            g.Status == GameStatus.WaitingForPlayers && 
            g.Players.Count < 2 &&
            g.Id >= 1000); // Solo partidas online

        if (waitingGame != null)
        {
            // Unirse a partida existente
            waitingGame.Players.Add(player);
            
            if (waitingGame.Players.Count == 2)
            {
                waitingGame.Status = GameStatus.InProgress;
            }

            await _gameRepository.UpdateAsync(waitingGame);
            return waitingGame;
        }
        else
        {
            // Crear nueva partida
            return await CreateNewOnlineGameAsync(player);
        }
    }

    private async Task<Game> CreateNewOnlineGameAsync(Player player)
    {
        var game = new Game
        {
            Id = Interlocked.Increment(ref _nextGameId),
            Status = GameStatus.WaitingForPlayers,
            CreatedAt = DateTime.UtcNow
        };

        game.Players.Add(player);

        var random = new Random();
        var equationParams = new EquationParams
        {
            TermCount = 2,
            VariableCount = 1,
            Operations = new List<string> { "+", "-" },
            ExpectedResult = random.Next(0, 2) == 0 ? "MAYOR" : "MENOR",
            OptionsCount = 4,
            OptionRangeMin = -10,
            OptionRangeMax = 10,
            NumberRangeMin = -10,
            NumberRangeMax = 10,
            TimePerEquation = 10
        };

        var allQuestions = _questionProvider.GetQuestions(equationParams, game.MaxQuestions);
        game.Questions = allQuestions;
        game.ExpectedResult = equationParams.ExpectedResult;

        await _gameRepository.AddAsync(game);
        return game;
    }
}