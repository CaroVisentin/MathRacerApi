using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para que un jugador se una a una partida ya creada
/// </summary>
public class JoinCreatedGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IPowerUpService _powerUpService;
    private readonly ILogger<JoinCreatedGameUseCase> _logger;

    public JoinCreatedGameUseCase(
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        IPowerUpService powerUpService,
        ILogger<JoinCreatedGameUseCase> logger)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _powerUpService = powerUpService;
        _logger = logger;
    }

    /// <summary>
    /// Une a un jugador a una partida existente.
    /// Si el jugador ya existe (mismo FirebaseUid), actualiza su ConnectionId.
    /// Si la partida llega a 2 jugadores, cambia a InProgress.
    /// </summary>
    public async Task<Game> ExecuteAsync(int gameId, string firebaseUid, string connectionId, string? password = null)
    {
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new ValidationException("UID de Firebase es requerido");

        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ValidationException("ConnectionId de SignalR es requerido");

        // Obtener partida
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            throw new NotFoundException("Game", gameId);

        // Validar estado de la partida
        if (game.Status != GameStatus.WaitingForPlayers)
            throw new ValidationException($"La partida no está disponible (estado: {game.Status})");

        // Validar contraseña para partidas privadas
        if (game.IsPrivate && game.Password != password)
            throw new ValidationException("Contraseña incorrecta");

        // Validar capacidad máxima
        if (game.Players.Count >= 2)
            throw new ValidationException("La partida está llena");

        // Obtener perfil del jugador
        var playerProfile = await _playerRepository.GetByUidAsync(firebaseUid);
        if (playerProfile == null)
            throw new NotFoundException("Perfil de jugador no encontrado");

        // BUSCAR SI EL JUGADOR YA EXISTE EN LA PARTIDA (mismo FirebaseUid)
        var existingPlayer = game.Players.FirstOrDefault(p => p.Uid == firebaseUid);

        if (existingPlayer != null)
        {
            // ACTUALIZAR ConnectionId del jugador existente
            _logger.LogInformation(
                $"Jugador {existingPlayer.Name} (Uid: {firebaseUid}) ya existe en partida {gameId}. " +
                $"Actualizando ConnectionId: {existingPlayer.ConnectionId} -> {connectionId}");

            existingPlayer.ConnectionId = connectionId;

            // Si es el creador y no se había asignado, asignarlo ahora
            if (game.CreatorPlayerId == null)
            {
                game.CreatorPlayerId = existingPlayer.Id;
                _logger.LogInformation($"Asignado creador de partida {gameId}: PlayerId {existingPlayer.Id}");
            }
        }
        else
        {
            // CREAR NUEVO JUGADOR usando el ID del PlayerProfile de BD
            var newPlayer = new Player
            {
                Id = playerProfile.Id, 
                Name = playerProfile.Name,
                Uid = firebaseUid,
                ConnectionId = connectionId,
                CorrectAnswers = 0,
                IndexAnswered = 0,
                Position = 0,
                IsReady = false,
                EquippedCar = playerProfile.Car,
                EquippedCharacter = playerProfile.Character,
                EquippedBackground = playerProfile.Background
            };

            // Otorgar power-ups iniciales
            newPlayer.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(newPlayer.Id);

            game.Players.Add(newPlayer);

            // Si es el primer jugador, es el creador
            if (game.Players.Count == 1 && game.CreatorPlayerId == null)
            {
                game.CreatorPlayerId = newPlayer.Id;
                _logger.LogInformation($"Primer jugador {newPlayer.Name} es el creador de partida {gameId}");
            }

            _logger.LogInformation(
                $"Nuevo jugador {newPlayer.Name} (ID: {newPlayer.Id}, Uid: {firebaseUid}) " +
                $"agregado a partida {gameId}");
        }

        // INICIAR JUEGO si hay 2 jugadores
        if (game.Players.Count == 2 && game.Status == GameStatus.WaitingForPlayers)
        {
            game.Status = GameStatus.InProgress;
            _logger.LogInformation($"Partida {gameId} iniciada con 2 jugadores: {string.Join(", ", game.Players.Select(p => p.Name))}");
        }

        // VERIFICAR DUPLICADOS (logging preventivo)
        var duplicateUids = game.Players
            .GroupBy(p => p.Uid)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var uid in duplicateUids)
        {
            _logger.LogWarning($"⚠️ Detectados jugadores duplicados con Uid: {uid} en partida {gameId}");
        }

        await _gameRepository.UpdateAsync(game);
        return game;
    }
}