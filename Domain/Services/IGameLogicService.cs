using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Services;

/// <summary>
/// Servicio de dominio que contiene lógica de juego compartida entre varios casos de uso
/// </summary>
public interface IGameLogicService
{
    /// <summary>
    /// Verifica si el juego debe terminar y actualiza el estado correspondiente
    /// </summary>
    /// <param name="game">La partida a verificar</param>
    /// <returns>True si el juego terminó</returns>
    bool CheckAndUpdateGameEndConditions(Game game);

    /// <summary>
    /// Calcula y asigna las posiciones de los jugadores basado en respuestas correctas
    /// </summary>
    /// <param name="game">La partida</param>
    void UpdatePlayerPositions(Game game);

    /// <summary>
    /// Verifica si un jugador puede responder (no está en penalización)
    /// </summary>
    /// <param name="player">El jugador a verificar</param>
    /// <returns>True si puede responder</returns>
    bool CanPlayerAnswer(Player player);

    /// <summary>
    /// Aplica la lógica de respuesta correcta/incorrecta a un jugador
    /// </summary>
    /// <param name="player">El jugador</param>
    /// <param name="isCorrect">Si la respuesta fue correcta</param>
    void ApplyAnswerResult(Player player, bool isCorrect);
}