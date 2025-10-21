using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para obtener la siguiente pregunta en una partida online
/// </summary>
public class GetNextOnlineQuestionUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IPowerUpService _powerUpService;

    public GetNextOnlineQuestionUseCase(IGameRepository gameRepository, IPowerUpService powerUpService)
    {
        _gameRepository = gameRepository;
        _powerUpService = powerUpService;
    }

    public async Task<Question?> ExecuteAsync(int gameId, int playerId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null) return null;

        var player = game.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return null;

        int nextIndex = player.IndexAnswered;
        if (nextIndex >= game.Questions.Count) return null;

        var question = game.Questions[nextIndex];
        
        // Aplicar efectos de ShuffleRival si hay alguno activo para este jugador
        if (game.PowerUpsEnabled)
        {
            var shuffleEffect = game.ActiveEffects.FirstOrDefault(e => 
                e.Type == PowerUpType.ShuffleRival && 
                e.TargetPlayerId == playerId && 
                e.IsActive && e.Properties.ContainsKey("Options"));

            if (shuffleEffect != null)
            {
                // Crear una copia de la pregunta con las opciones precomputadas
                var shuffledQuestion = new Question
                {
                    Id = question.Id,
                    Equation = question.Equation,
                    CorrectAnswer = question.CorrectAnswer,
                    Options = shuffleEffect.Properties["Options"] as List<int> ?? question.Options
                };

                // Desactivar efecto después de usarlo
                shuffleEffect.QuestionsRemaining--;
                if (shuffleEffect.QuestionsRemaining <= 0)
                {
                    shuffleEffect.IsActive = false;
                }

                // Actualizar el juego
                await _gameRepository.UpdateAsync(game);

                return shuffledQuestion;
            }
        }

        return question;
    }
}