using System;
using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// Respuesta al iniciar una partida individual
/// </summary>
public class StartSoloGameResponseDto
{
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int LevelId { get; set; }
    
    // Configuración del juego
    public int TotalQuestions { get; set; }
    public int TimePerEquation { get; set; }
    public int LivesRemaining { get; set; }
    
    // Primera pregunta
    public QuestionDto CurrentQuestion { get; set; } = new();
    
    public DateTime GameStartedAt { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Equation { get; set; } = string.Empty;
    public List<int> Options { get; set; } = new();
    public DateTime StartedAt { get; set; }
}