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

    public SubmitSoloAnswerUseCase(
        ISoloGameRepository soloGameRepository, 
        IEnergyRepository energyRepository,
        GrantLevelRewardUseCase grantLevelRewardUseCase)
    {
        _soloGameRepository = soloGameRepository;
        _energyRepository = energyRepository;
        _grantLevelRewardUseCase = grantLevelRewardUseCase;
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

        // Procesar resultado
        if (isCorrect)
        {
            game.PlayerPosition++;
            game.CorrectAnswers++;
            
            // Verificar si el jugador ganó
            if (game.PlayerPosition >= game.TotalQuestions)
            {
                game.Status = SoloGameStatus.PlayerWon;
                game.GameFinishedAt = DateTime.UtcNow;
                
                // Otorgar recompensas usando el caso de uso dedicado
                await _grantLevelRewardUseCase.ExecuteAsync(game.PlayerId, game.LevelId, game.WorldId);
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

        UpdateMachinePosition(game);

        if (game.MachinePosition >= game.TotalQuestions && game.Status == SoloGameStatus.InProgress)
        {
            game.Status = SoloGameStatus.MachineWon; 
            game.GameFinishedAt = DateTime.UtcNow;
        }

        await _soloGameRepository.UpdateAsync(game);

        return new SoloAnswerResult
        {
            Game = game,
            IsCorrect = isCorrect,
            CorrectAnswer = correctAnswer,
            PlayerAnswer = answer
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
