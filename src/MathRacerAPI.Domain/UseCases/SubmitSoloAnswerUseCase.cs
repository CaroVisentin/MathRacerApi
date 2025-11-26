using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para procesar la respuesta del jugador en modo individual
/// </summary>
public class SubmitSoloAnswerUseCase
{
    private readonly ISoloGameRepository _soloGameRepository;
    private readonly IEnergyRepository _energyRepository;
    private readonly GrantLevelRewardUseCase _grantLevelRewardUseCase;
    private readonly ILevelRepository _levelRepository;
    private readonly IPlayerRepository _playerRepository;

    public SubmitSoloAnswerUseCase(
        ISoloGameRepository soloGameRepository, 
        IEnergyRepository energyRepository,
        GrantLevelRewardUseCase grantLevelRewardUseCase,
        ILevelRepository levelRepository,
        IPlayerRepository playerRepository)
    {
        _soloGameRepository = soloGameRepository;
        _energyRepository = energyRepository;
        _grantLevelRewardUseCase = grantLevelRewardUseCase;
        _levelRepository = levelRepository;
        _playerRepository = playerRepository;
    }

    public async Task<SoloAnswerResult> ExecuteAsync(int gameId, int answer, string requestingPlayerUid)
    {
        var game = await _soloGameRepository.GetByIdAsync(gameId);
        
        if (game == null)
        {
            throw new NotFoundException($"Partida con ID {gameId} no encontrada");
        }

        // Validar que el jugador que envía la respuesta es el dueño de la partida
        if (game.PlayerUid != requestingPlayerUid)
        {
            throw new BusinessException("No tienes permiso para enviar respuestas a esta partida");
        }

        if (game.Status != SoloGameStatus.InProgress)
        {
            throw new BusinessException("La partida ya finalizó");
        }

        // Obtener pregunta actual
        if (game.CurrentQuestionIndex >= game.Questions.Count)
        {
            throw new BusinessException("No hay más preguntas disponibles");
        }

        var currentQuestion = game.Questions[game.CurrentQuestionIndex];
        var correctAnswer = currentQuestion.CorrectAnswer;
        bool isCorrect = correctAnswer == answer;

        // Verificar si se excedió el tiempo permitido
        if (game.LastAnswerTime.HasValue)
        {
            var timeSinceLastAnswer = (DateTime.UtcNow - game.LastAnswerTime.Value).TotalSeconds;
            
            if (timeSinceLastAnswer > game.TimePerEquation + game.ReviewTimeSeconds)
            {
                isCorrect = false;
            }
        }
        else
        {
            var timeSinceGameStart = (DateTime.UtcNow - game.GameStartedAt).TotalSeconds;
            
            if (timeSinceGameStart > game.TimePerEquation)
            {
                isCorrect = false;
            }
        }

        bool shouldOpenChest = false;
        int progressIncrement = 1;
        int coinsEarned = 0;

        // Aplicar doble progreso si está activo y la respuesta es correcta
        if (isCorrect && game.HasDoubleProgressActive)
        {
            progressIncrement = 2;
            game.HasDoubleProgressActive = false; // Desactivar después de usar
        }

        // Procesar resultado
        if (isCorrect)
        {
            game.PlayerPosition += progressIncrement;
            game.CorrectAnswers++;
            
            // Verificar si el jugador ganó
            if (game.PlayerPosition >= game.TotalQuestions)
            {
                game.Status = SoloGameStatus.PlayerWon;
                game.GameFinishedAt = DateTime.UtcNow;
                
                // Otorgar recompensas usando el caso de uso dedicado y obtener monedas
                coinsEarned = await _grantLevelRewardUseCase.ExecuteAsync(game.PlayerId, game.LevelId, game.WorldId);

                // Verificar si es un nivel nuevo (primera vez que lo completa)
                var player = await _playerRepository.GetByIdAsync(game.PlayerId);
                bool isNewLevel = player != null && game.LevelId > (player.LastLevelId ?? 0);

                // Verificar si es el último nivel del mundo (nivel 15) Y es la primera vez
                if (isNewLevel)
                {
                    var level = await _levelRepository.GetByIdAsync(game.LevelId);
                    if (level != null && level.Number == 15)
                    {
                        // Indicar que debe abrir el cofre de finalización del mundo
                        shouldOpenChest = true;
                    }
                }
            }
        }
        else
        {
            game.LivesRemaining--;

            if (game.LivesRemaining <= 0)
            {
                game.Status = SoloGameStatus.PlayerLost; 
                game.GameFinishedAt = DateTime.UtcNow;
                await _energyRepository.ConsumeEnergyAsync(game.PlayerId);
            }
        }

        game.LastAnswerTime = DateTime.UtcNow;
        game.CurrentQuestionIndex++;

        // Limpiar opciones modificadas después de responder
        game.ModifiedOptions = null;

        UpdateMachinePosition(game);

        if (game.MachinePosition >= game.TotalQuestions && game.Status == SoloGameStatus.InProgress)
        {
            game.Status = SoloGameStatus.MachineWon; 
            game.GameFinishedAt = DateTime.UtcNow;
        }

        await _soloGameRepository.UpdateAsync(game);

        // Obtener el saldo actualizado de monedas del jugador
        var updatedPlayer = await _playerRepository.GetByIdAsync(game.PlayerId);
        var remainingCoins = updatedPlayer?.Coins ?? 0;

        return new SoloAnswerResult
        {
            Game = game,
            IsCorrect = isCorrect,
            CorrectAnswer = correctAnswer,
            PlayerAnswer = answer,
            ShouldOpenWorldCompletionChest = shouldOpenChest,
            ProgressIncrement = progressIncrement,
            CoinsEarned = coinsEarned,
            RemainingCoins = remainingCoins
        };
    }

    /// <summary>
    /// Actualiza la posición de la máquina basándose en el tiempo total transcurrido
    /// </summary>
    private void UpdateMachinePosition(SoloGame game)
    {
        var elapsedTime = (DateTime.UtcNow - game.GameStartedAt).TotalSeconds;
        var totalEstimatedTime = game.TotalEstimatedTime;
        
        var progress = elapsedTime / totalEstimatedTime;
        game.MachinePosition = (int)(progress * game.TotalQuestions);
        
        game.MachinePosition = Math.Min(game.MachinePosition, game.TotalQuestions);
    }
}
