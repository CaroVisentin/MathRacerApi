using Microsoft.AspNetCore.SignalR;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Presentation.DTOs.SignalR;

namespace MathRacerAPI.Presentation.Hubs;

/// <summary>
/// Hub de SignalR para manejar las comunicaciones en tiempo real del juego
/// </summary>
public class GameHub : Hub
{
    private readonly FindMatchUseCase _findMatchUseCase;
    private readonly JoinCreatedGameUseCase _joinCreatedGameUseCase;
    private readonly FindMatchWithMatchmakingUseCase _findMatchWithMatchmakingUseCase;
    private readonly ProcessOnlineAnswerUseCase _processAnswerUseCase;
    private readonly GetNextOnlineQuestionUseCase _getNextQuestionUseCase;
    private readonly IGameRepository _gameRepository;
    private readonly IPowerUpService _powerUpService;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        FindMatchUseCase findMatchUseCase,
        JoinCreatedGameUseCase joinCreatedGameUseCase,
        FindMatchWithMatchmakingUseCase findMatchWithMatchmakingUseCase,
        ProcessOnlineAnswerUseCase processAnswerUseCase,
        GetNextOnlineQuestionUseCase getNextQuestionUseCase,
        IGameRepository gameRepository,
        IPowerUpService powerUpService,
        ILogger<GameHub> logger)
    {
        _findMatchUseCase = findMatchUseCase;
        _joinCreatedGameUseCase = joinCreatedGameUseCase;
        _findMatchWithMatchmakingUseCase = findMatchWithMatchmakingUseCase;
        _processAnswerUseCase = processAnswerUseCase;
        _getNextQuestionUseCase = getNextQuestionUseCase;
        _gameRepository = gameRepository;
        _powerUpService = powerUpService;
        _logger = logger;
    }

    /// <summary>
    /// Busca una partida disponible o crea una nueva usando matchmaking FIFO (First In, First Out).
    /// El sistema FIFO empareja al primer jugador disponible sin considerar habilidades.
    /// </summary>
    /// <param name="playerUid">UID del jugador para obtener su nombre real</param>
    public async Task FindMatch(string playerUid)
    {
        try
        {
            _logger.LogInformation($"FindMatch iniciado para UID: {playerUid} ({Context.ConnectionId})");
            
            var game = await _findMatchUseCase.ExecuteAsync(Context.ConnectionId, playerUid);
            
            _logger.LogInformation($"FindMatchUseCase completado. Partida {game.Id} con {game.Players.Count} jugadores");
            
            var player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                _logger.LogError($"No se pudo encontrar el jugador con UID {playerUid} en la partida {game.Id}");
                await Clients.Caller.SendAsync("Error", "Error al crear jugador");
                return;
            }
            
            _logger.LogInformation($"Jugador encontrado: {player.Name} (ID: {player.Id}, ConnectionId: {player.ConnectionId})");
            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Game_{game.Id}");
            _logger.LogInformation($"Jugador {player.Name} agregado al grupo Game_{game.Id}");

            await NotifyAllPlayersInGame(game.Id);

            _logger.LogInformation($"Jugador {player.Name} ({Context.ConnectionId}) procesado completamente para partida {game.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al buscar partida para el jugador con UID {playerUid}");
            await Clients.Caller.SendAsync("Error", "Error al buscar partida");
        }
    }

    /// <summary>
    /// Busca una partida usando matchmaking basado en puntos de ranking.
    /// El sistema de ranking empareja jugadores con habilidades similares usando tolerancias adaptativas
    /// para crear partidas equilibradas y competitivas.
    /// </summary>
    /// <param name="playerUid">UID del jugador para obtener sus puntos y nombre real</param>
    public async Task FindMatchWithMatchmaking(string playerUid)
    {
        try
        {
            _logger.LogInformation($"FindMatchWithMatchmaking iniciado para UID: {playerUid} ({Context.ConnectionId})");
            
            var game = await _findMatchWithMatchmakingUseCase.ExecuteAsync(Context.ConnectionId, playerUid);
            
            _logger.LogInformation($"FindMatchWithMatchmakingUseCase completado. Partida {game.Id} con {game.Players.Count} jugadores");
            
            // Encontrar el jugador recién creado
            var player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                _logger.LogError($"No se pudo encontrar el jugador con UID {playerUid} en la partida {game.Id}");
                await Clients.Caller.SendAsync("Error", "Error al crear jugador");
                return;
            }
            
            _logger.LogInformation($"Jugador encontrado: {player.Name} (ID: {player.Id}, ConnectionId: {player.ConnectionId})");
            
            // Unir al jugador al grupo de la partida
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Game_{game.Id}");
            _logger.LogInformation($"Jugador {player.Name} agregado al grupo Game_{game.Id}");

            // Notificar a CADA jugador individualmente con su pregunta específica
            _logger.LogInformation($"Iniciando notificación a todos los jugadores de la partida {game.Id}");
            await NotifyAllPlayersInGame(game.Id);

            _logger.LogInformation($"Jugador {player.Name} ({Context.ConnectionId}) procesado completamente para partida {game.Id} con matchmaking");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al buscar partida con matchmaking para el jugador con UID {playerUid}");
            await Clients.Caller.SendAsync("Error", "Error al buscar partida con matchmaking");
        }
    }

    /// <summary>
    /// Une al jugador autenticado a una partida ya creada
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <param name="password">Contraseña (opcional, solo para partidas privadas)</param>
    public async Task JoinGame(int gameId, string? password = null)
    {
        try
        {
             // Obtener el UID de Firebase del contexto (inyectado por middleware)
            // var firebaseUid = Context.Items["FirebaseUid"] as string;
            var http = Context.GetHttpContext();
            var firebaseUid = http?.Items["FirebaseUid"] as string;


            if (string.IsNullOrEmpty(firebaseUid))
            {
                await Clients.Caller.SendAsync("Error", "Autenticación requerida para unirse a la partida");
                return;
            }

            _logger.LogInformation($"JoinGame iniciado para gameId: {gameId}, uid: {firebaseUid}, connectionId: {Context.ConnectionId}");

            // Ejecutar caso de uso
            var game = await _joinCreatedGameUseCase.ExecuteAsync(gameId, firebaseUid, Context.ConnectionId, password);

            _logger.LogInformation($"Jugador unido exitosamente a partida {gameId}. Total jugadores: {game.Players.Count}");

            // Agregar al grupo de SignalR
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Game_{game.Id}");

            // Notificar a todos los jugadores de la partida
            await NotifyAllPlayersInGame(game.Id);

            _logger.LogInformation($"Jugador con uid {firebaseUid} procesado completamente para partida {gameId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al unirse a la partida {gameId}");
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    /// <summary>
    /// Procesa la respuesta de un jugador
    /// </summary>
    public async Task SendAnswer(int gameId, int playerId, int answer)
    {
        try
        {
            var game = await _processAnswerUseCase.ExecuteAsync(gameId, playerId, answer);
            
            if (game == null)
            {
                await Clients.Caller.SendAsync("Error", "Partida no encontrada");
                return;
            }

            await NotifyAllPlayersInGame(gameId);

            _logger.LogInformation($"Respuesta procesada para jugador {playerId} en partida {gameId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al procesar respuesta del jugador {playerId} en partida {gameId}");
            await Clients.Caller.SendAsync("Error", "Error al procesar respuesta");
        }
    }

    /// <summary>
    /// Activa un power-up de un jugador
    /// </summary>
    public async Task UsePowerUp(int gameId, int playerId, PowerUpType powerUpType)
    {
        try
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                await Clients.Caller.SendAsync("Error", "Partida no encontrada");
                return;
            }

            if (game.Status != GameStatus.InProgress)
            {
                await Clients.Caller.SendAsync("Error", "La partida no está en progreso");
                return;
            }

            if (!game.PowerUpsEnabled)
            {
                await Clients.Caller.SendAsync("Error", "Los power-ups no están habilitados en esta partida");
                return;
            }

            var player = game.Players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Jugador no encontrado");
                return;
            }

            if (!_powerUpService.CanUsePowerUp(player, powerUpType))
            {
                await Clients.Caller.SendAsync("Error", "Power-up no disponible");
                return;
            }

            var activeEffect = _powerUpService.UsePowerUp(game, playerId, powerUpType);
            if (activeEffect == null)
            {
                await Clients.Caller.SendAsync("Error", "No se pudo activar el power-up");
                return;
            }

            await _gameRepository.UpdateAsync(game);

            var powerUpDto = new PowerUpUsedDto
            {
                GameId = gameId,
                PlayerId = playerId,
                PowerUpType = powerUpType,
                TargetPlayerId = activeEffect?.TargetPlayerId
            };

            await Clients.Group($"Game_{gameId}").SendAsync("PowerUpUsed", powerUpDto);
            await NotifyAllPlayersInGame(gameId);

            _logger.LogInformation($"Power-up {powerUpType} usado por jugador {playerId} en partida {gameId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al usar power-up {powerUpType} por jugador {playerId} en partida {gameId}");
            await Clients.Caller.SendAsync("Error", "Error al activar power-up");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Jugador desconectado: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
       

    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Jugador conectado: {Context.ConnectionId}");
        // var uid = Context.Items["FirebaseUid"] as string;
        var http = Context.GetHttpContext();
        var uid = http?.Items["FirebaseUid"] as string;
        await base.OnConnectedAsync();
    }

    private async Task NotifyAllPlayersInGame(int gameId)
    {
        try
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) 
            {
                _logger.LogWarning($"Partida {gameId} no encontrada al notificar jugadores");
                return;
            }

            _logger.LogInformation($"Notificando a {game.Players.Count} jugadores de la partida {gameId}");

            // ✅ FILTRAR jugadores con ConnectionId válido
            var validPlayers = game.Players
                .Where(p => !string.IsNullOrWhiteSpace(p.ConnectionId))
                .ToList();

            // ⚠️ LOG de jugadores sin conexión
            var invalidPlayers = game.Players
                .Where(p => string.IsNullOrWhiteSpace(p.ConnectionId))
                .ToList();

            foreach (var player in invalidPlayers)
            {
                _logger.LogWarning(
                    $"⚠️ Jugador {player.Name} (ID: {player.Id}, Uid: {player.Uid}) " +
                    $"sin ConnectionId válido en partida {gameId}");
            }

            if (validPlayers.Count == 0)
            {
                _logger.LogWarning($"⚠️ No hay jugadores con ConnectionId válido en partida {gameId}");
                return;
            }

            foreach (var player in validPlayers)
            {
                try
                {
                    Question? currentQuestion = null;
                    
                    if (game.Status == GameStatus.InProgress)
                    {
                        currentQuestion = await _getNextQuestionUseCase.ExecuteAsync(gameId, player.Id);
                        _logger.LogInformation($"Pregunta obtenida para {player.Name}: {currentQuestion?.Equation ?? "ninguna"}");
                    }
                    else
                    {
                        _logger.LogInformation($"Juego en estado {game.Status}, no se envía pregunta a {player.Name}");
                    }
                    
                    var gameSession = GameSession.FromGame(game, currentQuestion);
                    var gameUpdateDto = GameUpdateDto.FromGameSession(gameSession);

                    _logger.LogInformation(
                        $"Enviando GameUpdate a jugador {player.Name} " +
                        $"(ConnectionId: {player.ConnectionId}) - Status: {game.Status}");
                    
                    await Clients.Client(player.ConnectionId).SendAsync("GameUpdate", gameUpdateDto);
                    
                    _logger.LogInformation($"✅ GameUpdate enviado exitosamente a {player.Name}");
                }
                catch (Exception playerEx)
                {
                    _logger.LogError(playerEx, 
                        $"❌ Error al notificar al jugador {player.Name} " +
                        $"(ConnectionId: {player.ConnectionId})");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error general al notificar jugadores de la partida {gameId}");
        }
    }

}