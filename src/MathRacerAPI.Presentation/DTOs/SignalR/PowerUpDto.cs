namespace MathRacerAPI.Presentation.DTOs.SignalR;

/// <summary>
/// DTO para representar un power-up disponible
/// </summary>
public class PowerUpDto
{
    public int Id { get; set; }
    public int Type { get; set; } // PowerUpType como int
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public static PowerUpDto FromPowerUp(Domain.Models.PowerUp powerUp)
    {
        return new PowerUpDto
        {
            Id = powerUp.Id,
            Type = (int)powerUp.Type,
            Name = powerUp.Name,
            Description = powerUp.Description
        };
    }
}

/// <summary>
/// DTO para representar un efecto activo en el juego
/// </summary>
public class ActiveEffectDto
{
    public int Type { get; set; } // PowerUpType como int
    public int SourcePlayerId { get; set; }
    public int? TargetPlayerId { get; set; }
    public int QuestionsRemaining { get; set; }
    public bool IsActive { get; set; }
    
    public static ActiveEffectDto FromActiveEffect(Domain.Models.ActiveEffect effect)
    {
        return new ActiveEffectDto
        {
            Type = (int)effect.Type,
            SourcePlayerId = effect.SourcePlayerId,
            TargetPlayerId = effect.TargetPlayerId,
            QuestionsRemaining = effect.QuestionsRemaining,
            IsActive = effect.IsActive
        };
    }
}