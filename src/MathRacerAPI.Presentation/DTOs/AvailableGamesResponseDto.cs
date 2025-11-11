namespace MathRacerAPI.Presentation.DTOs;

/// <summary>
/// DTO de respuesta para la lista de partidas disponibles
/// </summary>
public class AvailableGamesResponseDto
{
    public List<AvailableGameDto> Games { get; set; } = new();
    public int TotalGames { get; set; }
    public int PublicGames { get; set; }
    public int PrivateGames { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para información de una partida disponible
/// </summary>
public class AvailableGameDto
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public bool RequiresPassword { get; set; }
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public bool IsFull { get; set; }
    public string Status { get; set; } = "Esperando jugadores";
}