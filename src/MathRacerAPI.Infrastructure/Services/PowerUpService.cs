using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Services;

namespace MathRacerAPI.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de lógica de power-ups
/// </summary>
public class PowerUpService : IPowerUpService
{
    private static int _nextPowerUpId = 1;
    private static int _nextEffectId = 1;

    public List<PowerUp> GrantInitialPowerUps(int playerId)
    {
        // Otorgar los 2 power-ups al inicio de la partida
        return new List<PowerUp>
        {
            CreatePowerUp(PowerUpType.DoublePoints),
            CreatePowerUp(PowerUpType.ShuffleRival)
        };
    }

    public ActiveEffect? UsePowerUp(Game game, int playerId, PowerUpType powerUpType, int? targetPlayerId = null)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return null;

    var powerUp = player.AvailablePowerUps.FirstOrDefault(p => p.Type == powerUpType);
    if (powerUp == null) return null;

        ActiveEffect? effect = null;

        switch (powerUpType)
        {
            case PowerUpType.DoublePoints:
                player.HasDoublePointsActive = true;
                effect = new ActiveEffect
                {
                    Id = Interlocked.Increment(ref _nextEffectId),
                    Type = PowerUpType.DoublePoints,
                    SourcePlayerId = playerId,
                    QuestionsRemaining = 1,
                    IsActive = true
                };
                break;
                
            case PowerUpType.ShuffleRival:
                var targetPlayer = targetPlayerId.HasValue ? 
                    game.Players.FirstOrDefault(p => p.Id == targetPlayerId.Value) :
                    game.Players.FirstOrDefault(p => p.Id != playerId);
                    
                if (targetPlayer != null)
                {
                    effect = new ActiveEffect
                    {
                        Id = Interlocked.Increment(ref _nextEffectId),
                        Type = PowerUpType.ShuffleRival,
                        SourcePlayerId = playerId,
                        TargetPlayerId = targetPlayer.Id,
                        QuestionsRemaining = 1,
                        IsActive = true
                    };
                    // Precomputar las opciones mezcladas para la pregunta actual del rival (aplicación inmediata)
                    var nextIndex = targetPlayer.IndexAnswered;
                    if (nextIndex >= game.Questions.Count)
                    {
                        // No hay pregunta actual del rival, no aplicar
                        effect = null;
                        break;
                    }

                    var targetQuestion = game.Questions[nextIndex];
                    var shuffled = GetShuffledOptions(targetQuestion.Options, targetQuestion.CorrectAnswer);
                    effect.Properties["Options"] = shuffled;
                }
                break;
        }

        // Si no se pudo crear un efecto (por ejemplo no había objetivo válido), no consumir el power-up
        if (effect == null)
        {
            return null;
        }

        // Remover power-up de la lista (solo se puede usar una vez por partida)
        player.AvailablePowerUps.Remove(powerUp);

        // Agregar efecto al juego
        game.ActiveEffects.Add(effect);

        return effect;
    }

    public void ProcessActiveEffects(Game game)
    {
        var effectsToRemove = new List<ActiveEffect>();

        foreach (var effect in game.ActiveEffects.Where(e => e.IsActive))
        {
            switch (effect.Type)
            {
                case PowerUpType.DoublePoints:
                    var player = game.Players.FirstOrDefault(p => p.Id == effect.SourcePlayerId);
                    if (player != null && effect.QuestionsRemaining <= 0)
                    {
                        player.HasDoublePointsActive = false;
                        effect.IsActive = false;
                        effectsToRemove.Add(effect);
                    }
                    break;
                    
                case PowerUpType.ShuffleRival:
                    if (effect.QuestionsRemaining <= 0)
                    {
                        effect.IsActive = false;
                        effectsToRemove.Add(effect);
                    }
                    break;
            }
        }

        // Remover efectos expirados
        foreach (var effect in effectsToRemove)
        {
            game.ActiveEffects.Remove(effect);
        }
    }

    public bool CanUsePowerUp(Player player, PowerUpType powerUpType)
    {
        var powerUp = player.AvailablePowerUps.FirstOrDefault(p => p.Type == powerUpType);
        return powerUp != null;
    }

    public List<int> GetShuffledOptions(List<int> originalOptions, int correctAnswer)
    {
        var shuffled = new List<int>(originalOptions);
        var random = new Random();
        
        // Mezclar usando Fisher-Yates shuffle
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        
        return shuffled;
    }

    private PowerUp CreatePowerUp(PowerUpType type)
    {
        var (name, description) = GetPowerUpInfo(type);
        
        return new PowerUp
        {
            Id = Interlocked.Increment(ref _nextPowerUpId),
            Type = type,
            Name = name,
            Description = description
        };
    }

    private static (string Name, string Description) GetPowerUpInfo(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.DoublePoints => ("Puntos Dobles", "La siguiente respuesta correcta cuenta como 2"),
            PowerUpType.ShuffleRival => ("Confundir Rival", "Cambia el orden de las opciones del oponente"),
            _ => ("Desconocido", "Power-up desconocido")
        };
    }
}