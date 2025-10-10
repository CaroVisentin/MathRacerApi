using Microsoft.AspNetCore.SignalR;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Presentation.DTOs.SignalR;

namespace MathRacerAPI.Presentation.Hubs;

/// <summary>
/// Hub de SignalR para manejar las comunicaciones en tiempo real del juego
/// </summary>
public class GameHub : Hub
{
    private readonly FindMatchUseCase _findMatchUseCase;
    private readonly ProcessOnlineAnswerUseCase _processAnswerUseCase;
    private readonly GetNextOnlineQuestionUseCase _getNextQuestionUseCase;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        FindMatchUseCase findMatchUseCase,
        ProcessOnlineAnswerUseCase processAnswerUseCase,
        GetNextOnlineQuestionUseCase getNextQuestionUseCase,
        IGameRepository gameRepository,
        ILogger<GameHub> logger)
    {
        _findMatchUseCase = findMatchUseCase;
        _processAnswerUseCase = processAnswerUseCase;
        _getNextQuestionUseCase = getNextQuestionUseCase;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    /// <summary>
    /// Busca una partida disponible o crea una nueva
    /// </summary>
    /// <param name="playerName">Nombre del jugador</param>
    public async Task FindMatch(string playerName)
    {
        try
        {
            _logger.LogInformation($"FindMatch iniciado para {playerName} ({Context.ConnectionId})");
            
            var game = await _findMatchUseCase.ExecuteAsync(playerName, Context.ConnectionId);
            
            _logger.LogInformation($"FindMatchUseCase completado. Partida {game.Id} con {game.Players.Count} jugadores");
            
            // Encontrar el jugador recién creado
            var player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                _logger.LogError($"No se pudo encontrar el jugador {playerName} en la partida {game.Id}");
                await Clients.Caller.SendAsync("Error", "Error al crear jugador");
                return;
            }
            
            _logger.LogInformation($"Jugador encontrado: {player.Name} (ID: {player.Id}, ConnectionId: {player.ConnectionId})");
            
            // Unir al jugador al grupo de la partida
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Game_{game.Id}");
            _logger.LogInformation($"Jugador {playerName} agregado al grupo Game_{game.Id}");

            // Notificar a CADA jugador individualmente con su pregunta específica
            _logger.LogInformation($"Iniciando notificación a todos los jugadores de la partida {game.Id}");
            await NotifyAllPlayersInGame(game.Id);

            _logger.LogInformation($"Jugador {playerName} ({Context.ConnectionId}) procesado completamente para partida {game.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al buscar partida para el jugador {playerName}");
            await Clients.Caller.SendAsync("Error", "Error al buscar partida");
        }
    }

    /// <summary>
    /// Procesa la respuesta de un jugador
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <param name="playerId">ID del jugador</param>
    /// <param name="answer">Respuesta del jugador</param>
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

            // Notificar a CADA jugador individualmente con su pregunta específica
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
    /// Maneja la desconexión de un jugador
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Jugador desconectado: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Maneja la conexión de un jugador
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Jugador conectado: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Notifica a todos los jugadores de una partida con sus preguntas específicas
    /// </summary>
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

            // Notificar a cada jugador individualmente
            foreach (var player in game.Players.Where(p => !string.IsNullOrEmpty(p.ConnectionId)))
            {
                try
                {
                    Question? currentQuestion = null;
                    
                    // Solo enviar pregunta si el juego ya comenzó (2 jugadores)
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

                    _logger.LogInformation($"Enviando GameUpdate a jugador {player.Name} ({player.ConnectionId}) - Status: {game.Status}");
                    await Clients.Client(player.ConnectionId).SendAsync("GameUpdate", gameUpdateDto);
                    
                    _logger.LogInformation($"GameUpdate enviado exitosamente a {player.Name}");
                }
                catch (Exception playerEx)
                {
                    _logger.LogError(playerEx, $"Error al notificar al jugador {player.Name} ({player.ConnectionId})");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al notificar jugadores de la partida {gameId}");
        }
    }
}