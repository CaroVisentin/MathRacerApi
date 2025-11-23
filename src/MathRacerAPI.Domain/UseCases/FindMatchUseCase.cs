using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para encontrar o crear una partida multijugador online usando matchmaking FIFO.
/// FIFO (First In, First Out): El primer jugador que busque partida será emparejado con el siguiente jugador que busque,
/// sin considerar habilidades o puntos de ranking. Es un sistema simple y rápido de emparejamiento.
/// </summary>
public class FindMatchUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly IGameLogicService _gameLogicService;
    private readonly IPowerUpService _powerUpService;
    private static int _nextGameId = 1000; // Solo para IDs de partidas

    public FindMatchUseCase(
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        GetQuestionsUseCase getQuestionsUseCase, 
        IGameLogicService gameLogicService,
        IPowerUpService powerUpService)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _getQuestionsUseCase = getQuestionsUseCase;
        _gameLogicService = gameLogicService;
        _powerUpService = powerUpService;
    }

    public async Task<Game> ExecuteAsync(string connectionId, string playerUid)
    {
        // Obtener el perfil del jugador para usar su nombre real
        var playerProfile = await _playerRepository.GetByUidAsync(playerUid);
        if (playerProfile == null)
        {
            throw new NotFoundException("Perfil de jugador no encontrado");
        }

        // Crear jugador usando el ID de la base de datos
        var player = new Player 
        { 
            Id = playerProfile.Id, 
            Name = playerProfile.Name,
            Uid = playerUid,
            ConnectionId = connectionId,
            EquippedCar = playerProfile.Car,
            EquippedCharacter = playerProfile.Character,
            EquippedBackground = playerProfile.Background
        };

        player.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(player.Id);

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
            CreatedAt = DateTime.UtcNow,
            PowerUpsEnabled = true,
            MaxPowerUpsPerPlayer = 3,
            CreatorPlayerId = player.Id 
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

        var allQuestions = await _getQuestionsUseCase.GetQuestions(equationParams, game.MaxQuestions);
        game.Questions = allQuestions;
        game.ExpectedResult = equationParams.ExpectedResult;

        await _gameRepository.AddAsync(game);
        return game;
    }
}