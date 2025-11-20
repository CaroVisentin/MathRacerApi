using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para encontrar partidas con sistema de matchmaking basado en puntos de ranking.
/// Matchmaking por Ranking: Empareja jugadores con niveles de habilidad similares basándose en sus puntos de ranking,
/// usando tolerancias adaptativas según su experiencia para crear partidas más equilibradas y competitivas.
/// </summary>
public class FindMatchWithMatchmakingUseCase
{
    private static readonly SemaphoreSlim _matchmakingLock = new SemaphoreSlim(1, 1);
    private readonly ILogger<FindMatchWithMatchmakingUseCase> _logger;
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
        IPowerUpService powerUpService,
        ILogger<FindMatchWithMatchmakingUseCase> logger)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _getQuestionsUseCase = getQuestionsUseCase;
        _gameLogicService = gameLogicService;
        _powerUpService = powerUpService;
        _logger = logger;
    }

    /// <summary>
    /// Encuentra una partida usando matchmaking basado en puntos
    /// </summary>
    /// <param name="connectionId">ID de conexión de SignalR</param>
    /// <param name="playerUid">UID del jugador para obtener sus puntos y nombre</param>
    /// <returns>Partida encontrada o creada</returns>
    //public async Task<Game> ExecuteAsync(string connectionId, string playerUid)
    //{
    //    // Obtener el perfil del jugador para sus puntos
    //    var playerProfile = await _playerRepository.GetByUidAsync(playerUid);
    //    if (playerProfile == null)
    //    {
    //        throw new NotFoundException("Perfil de jugador no encontrado");
    //    }

    //    var player = new Player 
    //    { 
    //        Id = Interlocked.Increment(ref _nextPlayerId),
    //        Name = playerProfile.Name,
    //        Uid = playerUid,
    //        ConnectionId = connectionId
    //    };

    //     player.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(player.Id);


    //    // Calcular rango de tolerancia basado en puntos del jugador
    //    var toleranceRange = CalculateToleranceRange(playerProfile.Points);

    //    // Buscar partidas esperando jugadores con matchmaking
    //    var availableGames = await _gameRepository.GetAllAsync();
    //    var compatibleGame = await FindCompatibleGame(availableGames, playerProfile.Points, toleranceRange);

    //    if (compatibleGame != null)
    //    {
    //        // Unirse a partida compatible
    //        compatibleGame.Players.Add(player);

    //        if (compatibleGame.Players.Count == 2)
    //        {
    //            compatibleGame.Status = GameStatus.InProgress;
    //        }

    //        await _gameRepository.UpdateAsync(compatibleGame);
    //        return compatibleGame;
    //    }
    //    else
    //    {
    //        // Crear nueva partida y almacenar información de matchmaking
    //        return await CreateNewMatchmakingGameAsync(player, playerProfile.Points);
    //    }
    //}
    public async Task<Game> ExecuteAsync(string connectionId, string playerUid)
    {
        // LOCK: Solo un jugador puede buscar/crear partida a la vez
        await _matchmakingLock.WaitAsync();

        try
        {
            return await ExecuteMatchmakingAsync(connectionId, playerUid);
        }
        finally
        {
            _matchmakingLock.Release();
        }
    }

    private async Task<Game> ExecuteMatchmakingAsync(string connectionId, string playerUid)
    {
        var playerProfile = await _playerRepository.GetByUidAsync(playerUid);
        if (playerProfile == null)
        {
            throw new NotFoundException("Perfil de jugador no encontrado");
        }

        _logger.LogInformation($"🔍 Matchmaking para {playerProfile.Name} (UID: {playerUid}, Puntos: {playerProfile.Points})");

        // Buscar partidas esperando jugadores
        var waitingGames = await _gameRepository.GetWaitingGames();

        _logger.LogInformation($"📋 Encontradas {waitingGames.Count} partidas esperando jugadores");

        // Verificar si el jugador ya está en alguna partida
        var existingGame = waitingGames.FirstOrDefault(g =>
            g.Players.Any(p => p.Uid == playerUid));

        if (existingGame != null)
        {
            _logger.LogInformation($"✅ Jugador ya está en la partida {existingGame.Id}");

            // Recargar la partida con tracking para poder modificarla
            var trackedGame = await _gameRepository.GetByIdAsync(existingGame.Id);
            if (trackedGame != null)
            {
                var existingPlayer = trackedGame.Players.First(p => p.Uid == playerUid);
                existingPlayer.ConnectionId = connectionId;
                await _gameRepository.UpdateAsync(trackedGame);
                return trackedGame;
            }
            return existingGame;
        }

        // Calcular tolerancia
        var toleranceRange = CalculateToleranceRange(playerProfile.Points);
        _logger.LogInformation($"📊 Tolerancia: ±{toleranceRange} puntos");

        // Buscar partida compatible
        Game? compatibleGame = null;

        foreach (var game in waitingGames)
        {
            var existingPlayer = game.Players.First();

            _logger.LogInformation($"🔎 Evaluando partida {game.Id} - Jugador: {existingPlayer.Name} (UID: {existingPlayer.Uid})");

            var existingPlayerProfile = await GetPlayerProfileByUid(existingPlayer.Uid);

            if (existingPlayerProfile != null)
            {
                var pointsDifference = Math.Abs(playerProfile.Points - existingPlayerProfile.Points);

                _logger.LogInformation($"📊 Diferencia: {pointsDifference} puntos (Tolerancia: {toleranceRange})");

                if (pointsDifference <= toleranceRange)
                {
                    compatibleGame = game;
                    _logger.LogInformation($"✅ Partida {game.Id} es compatible!");
                    break;
                }
            }
        }

        // Crear Player
        var player = new Player
        {
            Id = Interlocked.Increment(ref _nextPlayerId),
            Name = playerProfile.Name,
            Uid = playerUid,
            ConnectionId = connectionId
        };

        player.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(player.Id);

        if (compatibleGame != null)
        {
            // Recargar con tracking para poder modificar
            var trackedGame = await _gameRepository.GetByIdAsync(compatibleGame.Id);

            if (trackedGame == null)
            {
                _logger.LogWarning($"⚠️ No se pudo recargar la partida {compatibleGame.Id}, creando nueva");
                return await CreateNewMatchmakingGameAsync(player, playerProfile.Points);
            }

            // Verificar nuevamente que no esté llena (otro jugador pudo unirse mientras tanto)
            if (trackedGame.Players.Count >= 2)
            {
                _logger.LogWarning($"⚠️ Partida {trackedGame.Id} ya está llena, creando nueva");
                return await CreateNewMatchmakingGameAsync(player, playerProfile.Points);
            }

            trackedGame.Players.Add(player);

            if (trackedGame.Players.Count == 2)
            {
                trackedGame.Status = GameStatus.InProgress;
                _logger.LogInformation($"🎮 Partida {trackedGame.Id} iniciada con 2 jugadores");
            }

            await _gameRepository.UpdateAsync(trackedGame);
            return trackedGame;
        }
        else
        {
            _logger.LogInformation($"🆕 Creando nueva partida...");
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