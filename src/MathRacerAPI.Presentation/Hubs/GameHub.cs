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
    private readonly IGameInvitationRepository _invitationRepository;
    private readonly IPowerUpService _powerUpService;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        FindMatchUseCase findMatchUseCase,
        JoinCreatedGameUseCase joinCreatedGameUseCase,
        FindMatchWithMatchmakingUseCase findMatchWithMatchmakingUseCase,
        ProcessOnlineAnswerUseCase processAnswerUseCase,
        GetNextOnlineQuestionUseCase getNextQuestionUseCase,
        IGameRepository gameRepository,
        IGameInvitationRepository invitationRepository,
        IPowerUpService powerUpService,
        ILogger<GameHub> logger)
    {
        _findMatchUseCase = findMatchUseCase;
        _joinCreatedGameUseCase = joinCreatedGameUseCase;
        _findMatchWithMatchmakingUseCase = findMatchWithMatchmakingUseCase;
        _processAnswerUseCase = processAnswerUseCase;
        _getNextQuestionUseCase = getNextQuestionUseCase;
        _gameRepository = gameRepository;
        _invitationRepository = invitationRepository;
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
            _logger.LogInformation($"🔍 Verificando ConnectionIds después del UseCase:");
            foreach (var p in game.Players)
            {
                _logger.LogInformation($"   - {p.Name}: ConnectionId = {p.ConnectionId}, Context = {Context.ConnectionId}, Match = {p.ConnectionId == Context.ConnectionId}");
            }
            // Encontrar el jugador recién creado
            var player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player  == null)
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
    /// Une al jugador autenticado a una partida ya creada.
    /// Si la partida es por invitación y ambos jugadores están conectados, limpia la invitación.
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <param name="password">Contraseña (opcional, solo para partidas privadas)</param>
    public async Task JoinGame(int gameId, string? password = null)
    {
        try
        {
             // Obtener el UID de Firebase del contexto (inyectado por middleware)
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

            // LIMPIEZA DE INVITACIONES: Si la partida es por invitación y ya están ambos jugadores
            if (game.IsFromInvitation && game.Players.Count == 2 && game.Status == GameStatus.InProgress)
            {
                await CleanupGameInvitation(gameId);
            }

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
            // Actualizar ConnectionId del jugador actual antes de procesar respuesta
            var gameBeforeAnswer = await _gameRepository.GetByIdAsync(gameId);
            var playerInGame = gameBeforeAnswer.Players.FirstOrDefault(p => p.Id == playerId);
            if (playerInGame != null && playerInGame.ConnectionId != Context.ConnectionId)
            {
                playerInGame.ConnectionId = Context.ConnectionId;
                await _gameRepository.UpdateAsync(gameBeforeAnswer);
            }
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
        try
        {
            _logger.LogInformation($"Jugador desconectado: {Context.ConnectionId}");

            // Buscar TODAS las partidas del jugador desconectado
            var games = await _gameRepository.GetAllAsync();
            var playerGames = games.Where(g => 
                g.Players.Any(p => p.ConnectionId == Context.ConnectionId))
                .ToList();

            foreach (var game in playerGames)
            {
                var disconnectedPlayer = game.Players.First(p => p.ConnectionId == Context.ConnectionId);

                // CASO 1: Partida en progreso - Marcar rival como ganador
                if (game.Status == GameStatus.InProgress)
                {
                    var rivalPlayer = game.Players.FirstOrDefault(p => p.Id != disconnectedPlayer.Id);

                    if (rivalPlayer != null)
                    {
                        _logger.LogInformation(
                            $"Jugador {disconnectedPlayer.Name} se desconectó. " +
                            $"Marcando a {rivalPlayer.Name} como ganador en partida {game.Id}");

                        game.Status = GameStatus.Finished;
                        game.WinnerId = rivalPlayer.Id;
                        
                        if (rivalPlayer.FinishedAt == null)
                        {
                            rivalPlayer.FinishedAt = DateTime.UtcNow;
                        }

                        rivalPlayer.Position = 1;
                        disconnectedPlayer.Position = 2;

                        await _gameRepository.UpdateAsync(game);
                        await NotifyAllPlayersInGame(game.Id);

                        _logger.LogInformation(
                            $"✅ Partida {game.Id} finalizada. Ganador: {rivalPlayer.Name} (por desconexión)");
                    }
                }
                // CASO 2: Partida esperando jugadores - Eliminar o limpiar
                else if (game.Status == GameStatus.WaitingForPlayers)
                {
                    // Si es el único jugador (creador), ELIMINAR la partida
                    if (game.Players.Count == 1)
                    {
                        _logger.LogInformation(
                            $"🗑️ Eliminando partida {game.Id} ('{game.Name}') - " +
                            $"Creador {disconnectedPlayer.Name} se desconectó sin rival");

                        // Limpiar invitación si existe
                        if (game.IsFromInvitation)
                        {
                            await CleanupGameInvitation(game.Id);
                        }

                        await _gameRepository.DeleteAsync(game.Id);
                    }
                    // Si hay otro jugador esperando, remover solo al desconectado
                    else
                    {
                        _logger.LogInformation(
                            $"Removiendo jugador {disconnectedPlayer.Name} de partida {game.Id} " +
                            $"(quedan {game.Players.Count - 1} jugadores)");

                        game.Players.Remove(disconnectedPlayer);
                        await _gameRepository.UpdateAsync(game);

                        // Notificar al jugador restante
                        await NotifyAllPlayersInGame(game.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al manejar desconexión del jugador {Context.ConnectionId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Jugador conectado: {Context.ConnectionId}");
        var http = Context.GetHttpContext();
        var uid = http?.Items["FirebaseUid"] as string;
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Limpia la invitación de una partida eliminándola de la base de datos
    /// </summary>
    private async Task CleanupGameInvitation(int gameId)
    {
        try
        {
            _logger.LogInformation($"🧹 Iniciando limpieza de invitación para partida {gameId}");

            var invitation = await _invitationRepository.GetByGameIdAsync(gameId);
            
            if (invitation == null)
            {
                _logger.LogWarning($"⚠️ No se encontró invitación para la partida {gameId}");
                return;
            }

            // ELIMINAR en cualquiera de estos casos:
            // 1. Ambos jugadores conectados (partida en progreso)
            // 2. El creador abandonó (partida eliminada)
            // 3. Invitación pendiente o aceptada (limpieza por desconexión)
            await _invitationRepository.DeleteAsync(invitation.Id);
            _logger.LogInformation($"✅ Invitación {invitation.Id} eliminada para partida {gameId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error al limpiar invitación de partida {gameId}");
        }
    }

    private async Task NotifyAllPlayersInGame(int gameId)
    {
        try
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            
            _logger.LogInformation($"🔍 ConnectionIds al notificar:");
            foreach (var p in game.Players)
            {
                _logger.LogInformation($"   - {p.Name}: ConnectionId = {p.ConnectionId}");
            }
            if (game == null) 
            {
                _logger.LogWarning($"Partida {gameId} no encontrada al notificar jugadores");
                return;
            }

            _logger.LogInformation($"Notificando a {game.Players.Count} jugadores de la partida {gameId}");

            // FILTRAR jugadores con ConnectionId válido
            var validPlayers = game.Players
                .Where(p => !string.IsNullOrWhiteSpace(p.ConnectionId))
                .ToList();

            // LOG de jugadores sin conexión
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