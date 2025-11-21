using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para enviar una respuesta en modo infinito
/// </summary>
public class SubmitInfiniteAnswerUseCase
{
    private readonly IInfiniteGameRepository _infiniteGameRepository;

    public SubmitInfiniteAnswerUseCase(IInfiniteGameRepository infiniteGameRepository)
    {
        _infiniteGameRepository = infiniteGameRepository;
    }

    public async Task<InfiniteAnswerResult> ExecuteAsync(int gameId, int selectedAnswer)
    {
        // 1. Obtener partida
        var game = await _infiniteGameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            throw new NotFoundException($"Partida infinita con ID {gameId} no encontrada");
        }

        // 2. Validar estado del juego
        if (game.AbandonedAt != null)
        {
            throw new BusinessException("La partida ha sido abandonada");
        }

        // 3. Validar índice de pregunta
        if (game.CurrentQuestionIndex >= game.Questions.Count)
        {
            throw new BusinessException("No hay más preguntas disponibles en este lote");
        }

        // 4. Obtener pregunta actual
        var currentQuestion = game.Questions[game.CurrentQuestionIndex];

        // 5. Verificar respuesta
        var isCorrect = currentQuestion.CorrectAnswer == selectedAnswer;

        if (isCorrect)
        {
            game.CorrectAnswers++;
        }

        // 6. Avanzar al siguiente índice
        game.CurrentQuestionIndex++;
        game.LastAnswerTime = DateTime.UtcNow;

        // 7. Determinar si necesita nuevo lote (cada 9 preguntas)
        var needsNewBatch = game.CurrentQuestionIndex >= 9;

        // 8. Actualizar partida
        await _infiniteGameRepository.UpdateAsync(game);

        // 9. Retornar resultado
        return new InfiniteAnswerResult
        {
            IsCorrect = isCorrect,
            CorrectAnswer = currentQuestion.CorrectAnswer,
            TotalCorrectAnswers = game.CorrectAnswers,
            CurrentQuestionIndex = game.CurrentQuestionIndex,
            NeedsNewBatch = needsNewBatch
        };
    }
}