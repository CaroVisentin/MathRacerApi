using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Services;

namespace MathRacerAPI.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de lógica de juego
/// </summary>
public class GameLogicService : IGameLogicService
{
    public bool CheckAndUpdateGameEndConditions(Game game)
    {
        // Verificar si alguien ganó por puntos
        var winner = game.Players.FirstOrDefault(p => p.CorrectAnswers >= game.ConditionToWin);
        if (winner != null && game.Status != GameStatus.Finished)
        {
            if (winner.FinishedAt == null)
                winner.FinishedAt = DateTime.UtcNow;

            game.Status = GameStatus.Finished;
            game.WinnerId = winner.Id;
            return true;
        }

        // Marcar jugadores que terminaron todas las preguntas
        foreach (var player in game.Players.Where(p => p.IndexAnswered >= game.Questions.Count && p.FinishedAt == null))
        {
            player.FinishedAt = DateTime.UtcNow;
        }

        // Verificar si todos terminaron todas las preguntas
        if (game.Players.All(p => p.IndexAnswered >= game.Questions.Count) && game.Status != GameStatus.Finished)
        {
            var maxCorrect = game.Players.Max(p => p.CorrectAnswers);
            var winners = game.Players.Where(p => p.CorrectAnswers == maxCorrect).ToList();

            if (winners.Count == 1)
            {
                game.WinnerId = winners[0].Id;
            }
            else
            {
                // Desempate por tiempo
                var earliestFinish = winners.Min(p => p.FinishedAt ?? DateTime.MaxValue);
                var tieWinner = winners.First(p => p.FinishedAt == earliestFinish);
                game.WinnerId = tieWinner.Id;
            }

            game.Status = GameStatus.Finished;
            return true;
        }

        return false;
    }

    public void UpdatePlayerPositions(Game game)
    {
        var sortedPlayers = game.Players
            .OrderByDescending(p => p.CorrectAnswers)
            .ThenBy(p => p.FinishedAt ?? DateTime.MaxValue)
            .ToList();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            sortedPlayers[i].Position = i + 1;
        }
    }

    public bool CanPlayerAnswer(Player player)
    {
        return !player.PenaltyUntil.HasValue || DateTime.UtcNow >= player.PenaltyUntil.Value;
    }

    public void ApplyAnswerResult(Player player, bool isCorrect)
    {
        if (isCorrect)
        {
            player.CorrectAnswers++;
            player.PenaltyUntil = null; // Eliminar penalización si la tenía
        }
        else
        {
            player.PenaltyUntil = DateTime.UtcNow.AddSeconds(2); // Penalizar por 2 segundos
        }

        player.IndexAnswered++;
    }
}