using MathRacerAPI.Presentation.DTOs.SignalR;
using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// DTO de respuesta al usar un wildcard
/// </summary>
public class UseWildcardResponseDto
{
    public int WildcardId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RemainingQuantity { get; set; }
    
    // Para RemoveWrongOption (ID 1)
    public List<int>? ModifiedOptions { get; set; }
    
    // Para SkipQuestion (ID 2)
    public int? NewQuestionIndex { get; set; }
    public SoloQuestionDto? NewQuestion { get; set; }
    
    // Para DoubleProgress (ID 3)
    public bool DoubleProgressActive { get; set; }
}