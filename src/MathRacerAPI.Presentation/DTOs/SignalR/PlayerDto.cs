namespace MathRacerAPI.Presentation.DTOs.SignalR;

/// <summary>
/// DTO para representar el estado de un jugador en SignalR
/// </summary>
public class PlayerDto
{
    public int Id { get; set; }
    public string? Uid { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CorrectAnswers { get; set; }
    public int Position { get; set; }
    public bool IsReady { get; set; }
    public DateTime? PenaltyUntil { get; set; }
    public DateTime? FinishedAt { get; set; }
    public List<PowerUpDto> AvailablePowerUps { get; set; } = new();
    public bool HasDoublePointsActive { get; set; }
    
    // Productos equipados
    public EquippedProductDto? EquippedCar { get; set; }
    public EquippedProductDto? EquippedCharacter { get; set; }
    public EquippedProductDto? EquippedBackground { get; set; }
}