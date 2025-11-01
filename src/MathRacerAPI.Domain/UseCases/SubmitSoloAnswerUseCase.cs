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

    public SubmitSoloAnswerUseCase(ISoloGameRepository soloGameRepository, IEnergyRepository energyRepository)
    {
        _soloGameRepository = soloGameRepository;
        _energyRepository = energyRepository;
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
            
            // Si pasó más tiempo del permitido desde la última respuesta, penalizar
            if (timeSinceLastAnswer > game.TimePerEquation + game.ReviewTimeSeconds)
            {
                isCorrect = false; // Tratar como respuesta incorrecta por timeout
            }
        }
        else
        {
            // Es la primera pregunta, validar tiempo desde inicio del juego
            var timeSinceGameStart = (DateTime.UtcNow - game.GameStartedAt).TotalSeconds;
            
            if (timeSinceGameStart > game.TimePerEquation)
            {
                isCorrect = false; // Timeout en la primera pregunta
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
            }
        }
        else
        {
            game.LivesRemaining--;

            // Verificar si perdió todas las vidas
            if (game.LivesRemaining <= 0)
            {
                game.Status = SoloGameStatus.PlayerLost; 
                game.GameFinishedAt = DateTime.UtcNow;
                await _energyRepository.ConsumeEnergyAsync(game.PlayerId);
            }
        }

        // Marcar cuándo se respondió esta pregunta
        game.LastAnswerTime = DateTime.UtcNow;
        
        // Avanzar al siguiente índice
        game.CurrentQuestionIndex++;

        // Actualizar posición de la máquina basándose en tiempo total transcurrido
        UpdateMachinePosition(game);

        // Verificar si la máquina ganó
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
