namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Representa una partida en modo infinito
/// </summary>
public class InfiniteGame
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string PlayerUid { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;

    // Progreso del jugador
    public int CorrectAnswers { get; set; } = 0;
    public int CurrentQuestionIndex { get; set; } = 0;
    public int CurrentBatch { get; set; } = 0; 
    // Ecuaciones actuales
    public List<InfiniteQuestion> Questions { get; set; } = new();

    // Control de dificultad
    public int CurrentWorldId { get; set; } = 1;
    public int CurrentDifficultyStep { get; set; } = 0; 

    // Fechas
    public DateTime GameStartedAt { get; set; }
    public DateTime? LastAnswerTime { get; set; }
    public DateTime? AbandonedAt { get; set; } // null = en progreso, con fecha = abandonado

    // Propiedad de conveniencia para verificar si está activo
    public bool IsActive => AbandonedAt == null;
}
