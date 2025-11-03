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
    public string ResultType { get; set; } = string.Empty;

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
    public DateTime? LastAnswerTime { get; set; } 
    public DateTime? GameFinishedAt { get; set; }
    public int ReviewTimeSeconds { get; set; } = 3; 

    // Estado del juego
    public SoloGameStatus Status { get; set; } = SoloGameStatus.InProgress;

    // Tiempo total estimado para la máquina
    public int TotalEstimatedTime => (TotalQuestions + 3) * TimePerEquation;

    public List<PlayerProduct> PlayerProducts { get; set; } = new();
    public List<PlayerProduct> MachineProducts { get; set; } = new();

    // Wildcards disponibles y usados en la partida
    public List<PlayerWildcard> AvailableWildcards { get; set; } = new();
    public HashSet<int> UsedWildcardTypes { get; set; } = new(); // IDs de wildcards ya usados en esta partida
    public bool HasDoubleProgressActive { get; set; } = false; // Para el wildcard de doble progreso
    public List<int>? ModifiedOptions { get; set; } // Opciones modificadas por RemoveWrongOption
}

/// <summary>
/// Estados posibles de una partida individual
/// </summary>
public enum SoloGameStatus
{
    InProgress,
    PlayerWon,
    MachineWon,
    PlayerLost
}