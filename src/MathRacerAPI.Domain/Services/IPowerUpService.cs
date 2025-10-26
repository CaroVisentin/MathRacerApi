using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Services;

/// <summary>
/// Interfaz para el servicio de lógica de power-ups
/// </summary>
public interface IPowerUpService
{
    /// <summary>
    /// Otorga todos los power-ups iniciales a un jugador
    /// </summary>
    List<PowerUp> GrantInitialPowerUps(int playerId);
    
    /// <summary>
    /// Activa un power-up de un jugador
    /// </summary>
    ActiveEffect? UsePowerUp(Game game, int playerId, PowerUpType powerUpType, int? targetPlayerId = null);
    
    /// <summary>
    /// Procesa los efectos activos y los actualiza
    /// </summary>
    void ProcessActiveEffects(Game game);
    
    /// <summary>
    /// Verifica si un jugador puede usar un power-up específico
    /// </summary>
    bool CanUsePowerUp(Player player, PowerUpType powerUpType);
    
    /// <summary>
    /// Genera opciones mezcladas para el efecto ShuffleRival
    /// </summary>
    List<int> GetShuffledOptions(List<int> originalOptions, int correctAnswer);
}