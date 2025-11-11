namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Modelo de dominio para información de partidas disponibles
/// </summary>
public class AvailableGameInfo
{
    public int GameId { get; private set; }
    public string GameName { get; private set; }
    public bool IsPrivate { get; private set; }
    public bool RequiresPassword { get; private set; }
    public int CurrentPlayers { get; private set; }
    public int MaxPlayers { get; private set; }
    public string Difficulty { get; private set; }
    public string ExpectedResult { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatorName { get; private set; }
    public bool IsFull { get; private set; }

    public AvailableGameInfo(
        int gameId,
        string gameName,
        bool isPrivate,
        int currentPlayers,
        int maxPlayers,
        string difficulty,
        string expectedResult,
        DateTime createdAt,
        string creatorName)
    {
        GameId = gameId;
        GameName = gameName;
        IsPrivate = isPrivate;
        RequiresPassword = isPrivate;
        CurrentPlayers = currentPlayers;
        MaxPlayers = maxPlayers;
        Difficulty = difficulty;
        ExpectedResult = expectedResult;
        CreatedAt = createdAt;
        CreatorName = creatorName;
        IsFull = currentPlayers >= maxPlayers;
    }

    public static AvailableGameInfo FromGame(Game game)
    {
        var creator = game.Players.FirstOrDefault(p => p.Id == game.CreatorPlayerId);
        
        return new AvailableGameInfo(
            gameId: game.Id,
            gameName: game.Name,
            isPrivate: game.IsPrivate,
            currentPlayers: game.Players.Count,
            maxPlayers: 2,
            difficulty: "Unknown", 
            expectedResult: game.ExpectedResult,
            createdAt: game.CreatedAt,
            creatorName: creator?.Name ?? "Desconocido"
        );
    }
}