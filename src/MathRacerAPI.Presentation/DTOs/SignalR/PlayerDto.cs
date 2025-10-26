namespace MathRacerAPI.Presentation.DTOs.SignalR;

/// <summary>
/// DTO para representar el estado de un jugador en SignalR
/// </summary>
public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CorrectAnswers { get; set; }
    public int Position { get; set; }
    public bool IsReady { get; set; }
    public DateTime? PenaltyUntil { get; set; }
    public DateTime? FinishedAt { get; set; }
    public List<PowerUpDto> AvailablePowerUps { get; set; } = new();
    public bool HasDoublePointsActive { get; set; }
}