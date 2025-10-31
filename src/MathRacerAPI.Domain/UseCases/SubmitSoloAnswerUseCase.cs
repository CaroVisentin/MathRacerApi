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
        var isCorrect = correctAnswer == answer;

        // Validar si se excedió el tiempo de la pregunta actual
        if (game.CurrentQuestionStartedAt.HasValue)
        {
            var elapsedTime = (DateTime.UtcNow - game.CurrentQuestionStartedAt.Value).TotalSeconds;
            if (elapsedTime > game.TimePerEquation)
            {
                isCorrect = false; // Tratar como respuesta incorrecta
            }
        }

        if (isCorrect)
        {
            // Respuesta correcta: avanzar posición
            game.PlayerPosition++;
            game.CorrectAnswers++;
            
            // Verificar si el jugador ganó
            if (game.PlayerPosition >= game.TotalQuestions)
            {
                game.Status = SoloGameStatus.PlayerWon;
                game.WinnerId = game.PlayerId;
                game.GameFinishedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Respuesta incorrecta o tiempo agotado: perder vida
            game.LivesRemaining--;

            // Verificar si perdió todas las vidas
            if (game.LivesRemaining <= 0)
            {
                game.Status = SoloGameStatus.PlayerLost;
                game.GameFinishedAt = DateTime.UtcNow;

                // Consumir energía al perder el nivel
                await _energyRepository.ConsumeEnergyAsync(game.PlayerId);
            }
        }

        // Avanzar a la siguiente pregunta
        game.CurrentQuestionIndex++;
        game.CurrentQuestionStartedAt = DateTime.UtcNow;

        // Actualizar posición de la máquina
        UpdateMachinePosition(game);

        // Verificar si la máquina ganó
        if (game.MachinePosition >= game.TotalQuestions && game.Status == SoloGameStatus.InProgress)
        {
            game.Status = SoloGameStatus.MachineWon;
            game.WinnerId = -1;
            game.GameFinishedAt = DateTime.UtcNow;
        }

        await _soloGameRepository.UpdateAsync(game);

        // Devolver resultado con información de la respuesta
        return new SoloAnswerResult
        {
            Game = game,
            IsCorrect = isCorrect,
            CorrectAnswer = correctAnswer,
            PlayerAnswer = answer
        };
    }

    private void UpdateMachinePosition(SoloGame game)
    {
        var elapsedTime = (DateTime.UtcNow - game.GameStartedAt).TotalSeconds;
        var totalEstimatedTime = game.TotalEstimatedTime;
        
        var progress = elapsedTime / totalEstimatedTime;
        game.MachinePosition = (int)(progress * game.TotalQuestions);
        
        if (game.MachinePosition > game.TotalQuestions)
        {
            game.MachinePosition = game.TotalQuestions;
        }
    }
}
