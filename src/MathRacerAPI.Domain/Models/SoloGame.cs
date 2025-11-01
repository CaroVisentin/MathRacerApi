using System;
using System.Collections.Generic;

namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa una partida individual contra la máquina
/// </summary>
public class SoloGame
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string PlayerUid { get; set; } = string.Empty; 
    public string PlayerName { get; set; } = string.Empty;
    public int LevelId { get; set; }
    public int WorldId { get; set; }

    // Progreso del jugador
    public int PlayerPosition { get; set; } = 0;
    public int LivesRemaining { get; set; } = 3;
    public int CorrectAnswers { get; set; } = 0;
    public int CurrentQuestionIndex { get; set; } = 0;

    // Progreso de la máquina
    public int MachinePosition { get; set; } = 0;

    // Configuración del nivel
    public List<Question> Questions { get; set; } = new();
    public int TotalQuestions { get; set; } = 10;
    public int TimePerEquation { get; set; }

    // Control de tiempo
    public DateTime GameStartedAt { get; set; }
    public DateTime? CurrentQuestionStartedAt { get; set; }
    public DateTime? GameFinishedAt { get; set; }

    // Estado del juego
    public SoloGameStatus Status { get; set; } = SoloGameStatus.InProgress;
    public int? WinnerId { get; set; } // PlayerId si gana, -1 si gana la máquina

    public string? WinnerName => WinnerId switch
    {
        _ when WinnerId == PlayerId => PlayerName,
        -1 => "Máquina",
        _ => null
    };

    // Tiempo total estimado para la máquina
    public int TotalEstimatedTime => (TotalQuestions + 3) * TimePerEquation;

    public List<PlayerProduct> PlayerProducts { get; set; } = new();
    public List<PlayerProduct> MachineProducts { get; set; } = new();
}

/// <summary>
/// Estados posibles de una partida individual
/// </summary>
public enum SoloGameStatus
{
    InProgress,
    PlayerWon,
    MachineWon,
    PlayerLost // Sin vidas
}
