using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para que un jugador se una a una partida ya creada
/// </summary>
public class JoinCreatedGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IPowerUpService _powerUpService;

    public JoinCreatedGameUseCase(
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        IPowerUpService powerUpService)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _powerUpService = powerUpService;
    }

    /// <summary>
    /// Une a un jugador a una partida existente
    /// </summary>
    public async Task<Game> ExecuteAsync(int gameId, string firebaseUid, string connectionId, string? password = null)
    {
        // Obtener la partida
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            throw new NotFoundException("Game", gameId);
        }

        // Validar que la partida esté esperando jugadores
        if (game.Status != GameStatus.WaitingForPlayers)
        {
            throw new BusinessException("Esta partida ya comenzó o finalizó.");
        }

        // Validar contraseña si es privada
        if (game.IsPrivate)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new BusinessException("Esta partida es privada y requiere contraseña.");
            }

            if (string.IsNullOrWhiteSpace(game.Password))
            {
                throw new BusinessException("Error de configuración de la partida.");
            }

            if (game.Password != password)
            {
                throw new BusinessException("Contraseña incorrecta.");
            }
        }

        // Obtener el perfil del jugador
        var playerProfile = await _playerRepository.GetByUidAsync(firebaseUid);
        if (playerProfile == null)
        {
            throw new NotFoundException("Player", firebaseUid);
        }

        // Verificar que el jugador no intente unirse a su propia partida
        if (game.CreatorPlayerId == playerProfile.Id)
        {
            throw new BusinessException("No puedes unirte a tu propia partida. Ya eres parte de ella.");
        }

        // Verificar que el jugador no tenga otra partida activa
        var allGames = await _gameRepository.GetAllAsync();
        var activeGame = allGames.FirstOrDefault(g => 
            g.Id != gameId && // Excluir la partida actual
            g.Players.Any(p => p.Id == playerProfile.Id) && 
            (g.Status == GameStatus.WaitingForPlayers || g.Status == GameStatus.InProgress));

        if (activeGame != null)
        {
            throw new BusinessException(
                $"Ya tienes una partida activa (ID: {activeGame.Id}). " +
                $"Debes finalizarla o abandonarla antes de unirte a otra."
            );
        }

        // Verificar que haya espacio
        if (game.Players.Count >= 2)
        {
            throw new BusinessException("La partida está llena.");
        }

        // Crear nuevo jugador para la partida
        var player = new Player
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            ConnectionId = connectionId,
        };

        // Otorgar power-ups iniciales
        player.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(player.Id);

        game.Players.Add(player);

        // Si ya hay 2 jugadores, iniciar la partida
        if (game.Players.Count == 2)
        {
            game.Status = GameStatus.InProgress;
        }

        await _gameRepository.UpdateAsync(game);
        return game;
    }
}