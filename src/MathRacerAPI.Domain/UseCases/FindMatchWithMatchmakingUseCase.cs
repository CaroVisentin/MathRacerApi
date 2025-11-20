using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para encontrar partidas con sistema de matchmaking basado en puntos de ranking.
/// Matchmaking por Ranking: Empareja jugadores con niveles de habilidad similares basándose en sus puntos de ranking,
/// usando tolerancias adaptativas según su experiencia para crear partidas más equilibradas y competitivas.
/// </summary>
public class FindMatchWithMatchmakingUseCase
{
    private readonly ILogger<FindMatchWithMatchmakingUseCase> _logger;
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly IGameLogicService _gameLogicService;
    private readonly IPowerUpService _powerUpService;

    // IMPORTANTE: Lock estático para evitar race conditions
    private static readonly SemaphoreSlim _matchmakingLock = new SemaphoreSlim(1, 1);

    private static int _nextPlayerId = 1000;
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

        

        // Buscar partidas esperando jugadores
        var allGames = await _gameRepository.GetAllAsync();
        var waitingGames = allGames
            .Where(g =>
                g.Status == GameStatus.WaitingForPlayers &&
                g.Players.Count == 1 &&
                g.Id >= 1000 &&
                g.Players.First().Uid != playerUid) // NO unirse a su propia partida
            .OrderBy(g => g.CreatedAt) // Más antiguas primero
            .ToList();

        _logger.LogInformation($"📋 Encontradas {waitingGames.Count} partidas esperando jugadores");

        if (waitingGames.Count > 0)
        {
            _logger.LogInformation($"📋 Partidas disponibles:");
            foreach (var g in waitingGames)
            {
                var p = g.Players.FirstOrDefault();
                
            }
        }

        var player = new Player
        {
            Id = Interlocked.Increment(ref _nextPlayerId),
            Name = playerProfile.Name,
            Uid = playerUid,
            ConnectionId = connectionId
        };

        player.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(player.Id);

        // OPCIÓN 1: Intentar encontrar partida compatible por puntos
        var toleranceRange = CalculateToleranceRange(playerProfile.Points);
        _logger.LogInformation($"📊 Tolerancia: ±{toleranceRange} puntos");

        Game? compatibleGame = null;

        foreach (var game in waitingGames)
        {
            var existingPlayer = game.Players.First();
            _logger.LogInformation($"🔎 Evaluando partida {game.Id} - Jugador: {existingPlayer.Name}");

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
                else
                {
                    _logger.LogInformation($"❌ Partida {game.Id} fuera de tolerancia");
                }
            }
        }

        // OPCIÓN 2: Si no hay compatible, tomar CUALQUIER partida esperando (FALLBACK)
        if (compatibleGame == null && waitingGames.Count > 0)
        {
            _logger.LogInformation($"⚠️ No se encontró partida compatible. Uniendo a primera partida disponible...");
            compatibleGame = waitingGames.First();
            
        }

        if (compatibleGame != null)
        {
            

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
            _logger.LogInformation($"🆕 No hay partidas disponibles. Creando nueva partida para {player.Name}...");
            return await CreateNewMatchmakingGameAsync(player, playerProfile.Points);
        }
    }

    private static int CalculateToleranceRange(int playerPoints)
    {
        return playerPoints switch
        {
            <= 50 => 25,
            <= 150 => 30,
            <= 250 => 40,
            _ => 50
        };
    }

    private async Task<PlayerProfile?> GetPlayerProfileByUid(string uid)
    {
        try
        {
            return await _playerRepository.GetByUidAsync(uid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener perfil para UID: {uid}");
            return null;
        }
    }

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

        _logger.LogInformation($"🆕 Partida {game.Id} creada para {player.Name}");

        return game;
    }
}