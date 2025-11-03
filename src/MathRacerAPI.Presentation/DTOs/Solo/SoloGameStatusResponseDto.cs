using MathRacerAPI.Presentation.DTOs.SignalR;
using System;
using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// Estado completo de la partida individual (GET)
/// </summary>
public class SoloGameStatusResponseDto
{
    public int GameId { get; set; }
    public string Status { get; set; } = string.Empty; 
    
    // Progreso
    public int PlayerPosition { get; set; }
    public int MachinePosition { get; set; }
    public int LivesRemaining { get; set; }
    public int CorrectAnswers { get; set; }
    
    // Pregunta actual
    public QuestionDto? CurrentQuestion { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int TotalQuestions { get; set; }
    public int TimePerEquation { get; set; }
    
    // Tiempos
    public DateTime GameStartedAt { get; set; }
    public DateTime? GameFinishedAt { get; set; }
    public double ElapsedTime { get; set; }

    // Wildcards
    public List<WildcardDto> AvailableWildcards { get; set; } = new();
    public List<int> UsedWildcardTypes { get; set; } = new();
    public bool HasDoubleProgressActive { get; set; }
    public List<int>? ModifiedOptions { get; set; }
}