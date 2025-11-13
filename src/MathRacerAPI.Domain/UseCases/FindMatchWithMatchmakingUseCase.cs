using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para encontrar partidas con sistema de matchmaking basado en puntos de ranking.
/// Matchmaking por Ranking: Empareja jugadores con niveles de habilidad similares basándose en sus puntos de ranking,
/// usando tolerancias adaptativas según su experiencia para crear partidas más equilibradas y competitivas.
/// </summary>
public class FindMatchWithMatchmakingUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly IGameLogicService _gameLogicService;
    private readonly IPowerUpService _powerUpService;
    private static int _nextPlayerId = 1000; // Empezar desde 1000 para diferenciar del modo offline
    private static int _nextGameId = 1000;

    public FindMatchWithMatchmakingUseCase(
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

    /// <summary>
    /// Encuentra una partida usando matchmaking basado en puntos
    /// </summary>
    /// <param name="connectionId">ID de conexión de SignalR</param>
    /// <param name="playerUid">UID del jugador para obtener sus puntos y nombre</param>
    /// <returns>Partida encontrada o creada</returns>
    public async Task<Game> ExecuteAsync(string connectionId, string playerUid)
    {
        // Obtener el perfil del jugador para sus puntos
        var playerProfile = await _playerRepository.GetByUidAsync(playerUid);
        if (playerProfile == null)
        {
            throw new NotFoundException("Perfil de jugador no encontrado");
        }

        var player = new Player 
        { 
            Id = Interlocked.Increment(ref _nextPlayerId),
            Name = playerProfile.Name,
            Uid = playerUid,
            ConnectionId = connectionId
        };

        player.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(player.Id);

        // Calcular rango de tolerancia basado en puntos del jugador
        var toleranceRange = CalculateToleranceRange(playerProfile.Points);

        // Buscar partidas esperando jugadores con matchmaking
        var availableGames = await _gameRepository.GetAllAsync();
        var compatibleGame = await FindCompatibleGame(availableGames, playerProfile.Points, toleranceRange);

        if (compatibleGame != null)
        {
            // Unirse a partida compatible
            compatibleGame.Players.Add(player);
            
            if (compatibleGame.Players.Count == 2)
            {
                compatibleGame.Status = GameStatus.InProgress;
            }

            await _gameRepository.UpdateAsync(compatibleGame);
            return compatibleGame;
        }
        else
        {
            // Crear nueva partida y almacenar información de matchmaking
            return await CreateNewMatchmakingGameAsync(player, playerProfile.Points);
        }
    }

    /// <summary>
    /// Calcula el rango de tolerancia basado en los puntos del jugador
    /// </summary>
    private static int CalculateToleranceRange(int playerPoints)
    {
        // Rangos adaptativos basados en el análisis previo
        return playerPoints switch
        {
            <= 50 => 25,     // Principiante: ±25 puntos
            <= 150 => 30,    // Intermedio: ±30 puntos  
            <= 250 => 40,    // Avanzado: ±40 puntos
            _ => 50          // Experto: ±50 puntos
        };
    }

    /// <summary>
    /// Busca una partida compatible basada en puntos
    /// </summary>
    private async Task<Game?> FindCompatibleGame(List<Game> availableGames, int playerPoints, int tolerance)
    {
        var waitingGames = availableGames.Where(g => 
            g.Status == GameStatus.WaitingForPlayers && 
            g.Players.Count == 1 &&  // Solo juegos con 1 jugador esperando
            g.Id >= 1000)           // Solo partidas online
            .ToList();

        foreach (var game in waitingGames)
        {
            // Obtener los puntos del jugador que ya está en la partida
            var existingPlayer = game.Players.First();
            
            // Buscar el perfil del jugador existente usando su UID
            var existingPlayerProfile = await GetPlayerProfileByUid(existingPlayer.Uid);
            
            if (existingPlayerProfile != null)
            {
                var pointsDifference = Math.Abs(playerPoints - existingPlayerProfile.Points);
                
                // Si la diferencia está dentro de la tolerancia, es compatible
                if (pointsDifference <= tolerance)
                {
                    return game;
                }
            }
        }

        return null; // No se encontró partida compatible
    }

    /// <summary>
    /// Obtiene el perfil de un jugador por su UID almacenado en el objeto Player
    /// </summary>
    private async Task<PlayerProfile?> GetPlayerProfileByUid(string uid)
    {
        try
        {
            return await _playerRepository.GetByUidAsync(uid);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Crea una nueva partida con información de matchmaking
    /// </summary>
    private async Task<Game> CreateNewMatchmakingGameAsync(Player player, int creatorPoints)
    {
        var game = new Game
        {
            Id = Interlocked.Increment(ref _nextGameId),
            Status = GameStatus.WaitingForPlayers,
            CreatedAt = DateTime.UtcNow,
            PowerUpsEnabled = true,
            MaxPowerUpsPerPlayer = 3
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