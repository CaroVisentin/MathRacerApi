using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using Microsoft.Extensions.Logging;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para procesar respuestas en partidas multijugador online
/// </summary>
public class ProcessOnlineAnswerUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameLogicService _gameLogicService;
    private readonly IPowerUpService _powerUpService;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<ProcessOnlineAnswerUseCase> _logger;

    public ProcessOnlineAnswerUseCase(
        IGameRepository gameRepository,
        IGameLogicService gameLogicService,
        IPowerUpService powerUpService,
        IPlayerRepository playerRepository,
        ILogger<ProcessOnlineAnswerUseCase> logger)
    {
        _gameRepository = gameRepository;
        _gameLogicService = gameLogicService;
        _powerUpService = powerUpService;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<Game?> ExecuteAsync(int gameId, int playerId, int answer)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            _logger.LogWarning($"Partida {gameId} no encontrada");
            return null;
        }

        if (game.Status == GameStatus.Finished)
        {
            _logger.LogWarning($"Intento de respuesta en partida finalizada {gameId}");
            return game;
        }

        var player = game.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
        {
            _logger.LogWarning($"Jugador {playerId} no encontrado en partida {gameId}");
            return null;
        }

        // Verificar si el jugador puede responder (no está en penalización)
        if (!_gameLogicService.CanPlayerAnswer(player))
        {
            _logger.LogInformation($"Jugador {playerId} intentó responder durante penalización");
            return game;
        }

        int currentIndex = player.IndexAnswered;
        if (currentIndex >= game.Questions.Count)
        {
            _logger.LogWarning($"Jugador {playerId} intentó responder más preguntas de las disponibles");
            return game;
        }

        var question = game.Questions[currentIndex];
        bool isCorrect = question.CorrectAnswer == answer;

        // Aplicar resultado de la respuesta usando el servicio de dominio
        _gameLogicService.ApplyAnswerResult(player, isCorrect);

        // Procesar efectos activos de power-ups
        if (game.PowerUpsEnabled)
        {
            _powerUpService.ProcessActiveEffects(game);
        }

        // Actualizar posiciones de jugadores
        _gameLogicService.UpdatePlayerPositions(game);

        // Verificar condiciones de finalización
        var gameEnded = _gameLogicService.CheckAndUpdateGameEndConditions(game);
        if (gameEnded)
        {
            _logger.LogInformation($"Partida {gameId} terminada, ganador: jugador {game.WinnerId}");

            // Asignar puntos al ganador y restar a los perdedores
            if (game.WinnerId.HasValue)
            {
                var winnerProfile = await _playerRepository.GetByIdAsync(game.WinnerId.Value);
                if (winnerProfile != null)
                {
                    winnerProfile.Points += 10;
                    await _playerRepository.UpdateAsync(winnerProfile);
                }
                foreach (var p in game.Players.Where(x => x.Id != game.WinnerId.Value))
                {
                    var loserProfile = await _playerRepository.GetByIdAsync(p.Id);
                    if (loserProfile != null)
                    {
                        loserProfile.Points = Math.Max(0, loserProfile.Points - 5);
                        await _playerRepository.UpdateAsync(loserProfile);
                    }
                }
            }
        }

        _logger.LogInformation($"Respuesta {(isCorrect ? "correcta" : "incorrecta")} del jugador {playerId} en partida {gameId}");

        await _gameRepository.UpdateAsync(game);
        return game;
    }
}