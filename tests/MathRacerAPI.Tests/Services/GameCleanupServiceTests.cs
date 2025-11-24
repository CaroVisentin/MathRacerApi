using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Services;

namespace MathRacerAPI.Tests.Services;

/// <summary>
/// Tests unitarios para el servicio de limpieza de partidas abandonadas
/// </summary>
public class GameCleanupServiceTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<ILogger<GameCleanupService>> _loggerMock;
    private readonly IServiceProvider _serviceProvider;
    private readonly GameCleanupService _service;

    public GameCleanupServiceTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _loggerMock = new Mock<ILogger<GameCleanupService>>();

        // Configurar ServiceProvider para inyectar el repositorio mock
        var services = new ServiceCollection();
        services.AddScoped<IGameRepository>(_ => _gameRepositoryMock.Object);
        _serviceProvider = services.BuildServiceProvider();

        _service = new GameCleanupService(_serviceProvider, _loggerMock.Object);
    }

    #region Cleanup Tests

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WithNoGames_ShouldNotDeleteAnyGame()
    {
        // Arrange
        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game>());

        // Act
        await InvokeCleanupMethod();

        // Assert
        _gameRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WithRecentWaitingGame_ShouldNotDeleteGame()
    {
        // Arrange
        var recentGame = CreateGame(
            gameId: 1001,
            status: GameStatus.WaitingForPlayers,
            createdAt: DateTime.UtcNow.AddMinutes(-5)); // Creada hace 5 minutos (< 10 min timeout)

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { recentGame });

        // Act
        await InvokeCleanupMethod();

        // Assert
        _gameRepositoryMock.Verify(x => x.DeleteAsync(1001), Times.Never);
    }

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WithOldWaitingGame_ShouldDeleteGame()
    {
        // Arrange
        var abandonedGame = CreateGame(
            gameId: 1002,
            status: GameStatus.WaitingForPlayers,
            createdAt: DateTime.UtcNow.AddMinutes(-15)); // Creada hace 15 minutos (> 10 min timeout)

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { abandonedGame });

        // Act
        await InvokeCleanupMethod();

        // Assert
        _gameRepositoryMock.Verify(x => x.DeleteAsync(1002), Times.Once);
    }

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WithInProgressGame_ShouldNotDeleteGameRegardlessOfAge()
    {
        // Arrange
        var inProgressGame = CreateGame(
            gameId: 1003,
            status: GameStatus.InProgress,
            createdAt: DateTime.UtcNow.AddMinutes(-30)); // Muy vieja pero en progreso

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { inProgressGame });

        // Act
        await InvokeCleanupMethod();

        // Assert
        _gameRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WithFinishedGame_ShouldNotDeleteGameRegardlessOfAge()
    {
        // Arrange
        var finishedGame = CreateGame(
            gameId: 1004,
            status: GameStatus.Finished,
            createdAt: DateTime.UtcNow.AddMinutes(-30)); // Muy vieja pero terminada

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { finishedGame });

        // Act
        await InvokeCleanupMethod();

        // Assert
        _gameRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WithMixedGames_ShouldDeleteOnlyAbandonedWaitingGames()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-5)),  // NO eliminar (reciente)
            CreateGame(1002, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-15)), // SÍ eliminar (abandonada)
            CreateGame(1003, GameStatus.InProgress, DateTime.UtcNow.AddMinutes(-30)),        // NO eliminar (en progreso)
            CreateGame(1004, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-20)), // SÍ eliminar (abandonada)
            CreateGame(1005, GameStatus.Finished, DateTime.UtcNow.AddMinutes(-50))           // NO eliminar (terminada)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        await InvokeCleanupMethod();

        // Assert
        _gameRepositoryMock.Verify(x => x.DeleteAsync(1002), Times.Once);
        _gameRepositoryMock.Verify(x => x.DeleteAsync(1004), Times.Once);
        _gameRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WhenRepositoryThrowsException_ShouldLogErrorAndContinue()
    {
        // Arrange
        var abandonedGame = CreateGame(
            gameId: 1007,
            status: GameStatus.WaitingForPlayers,
            createdAt: DateTime.UtcNow.AddMinutes(-15));

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { abandonedGame });

        _gameRepositoryMock
            .Setup(x => x.DeleteAsync(1007))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var act = async () => await InvokeCleanupMethod();

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WithMultipleAbandonedGames_ShouldDeleteAllOfThem()
    {
        // Arrange
        var abandonedGames = new List<Game>
        {
            CreateGame(2001, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-11)),
            CreateGame(2002, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-15)),
            CreateGame(2003, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-20)),
            CreateGame(2004, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-30))
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(abandonedGames);

        // Act
        await InvokeCleanupMethod();

        // Assert
        _gameRepositoryMock.Verify(x => x.DeleteAsync(2001), Times.Once);
        _gameRepositoryMock.Verify(x => x.DeleteAsync(2002), Times.Once);
        _gameRepositoryMock.Verify(x => x.DeleteAsync(2003), Times.Once);
        _gameRepositoryMock.Verify(x => x.DeleteAsync(2004), Times.Once);
        _gameRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Exactly(4));
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WhenDeletingGame_ShouldLogInformation()
    {
        // Arrange
        var abandonedGame = CreateGame(
            gameId: 1008,
            status: GameStatus.WaitingForPlayers,
            createdAt: DateTime.UtcNow.AddMinutes(-15));

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { abandonedGame });

        // Act
        await InvokeCleanupMethod();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Limpiando partida abandonada 1008")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CleanupAbandonedGamesAsync_WhenDeletesMultipleGames_ShouldLogSummary()
    {
        // Arrange
        var abandonedGames = new List<Game>
        {
            CreateGame(3001, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-15)),
            CreateGame(3002, GameStatus.WaitingForPlayers, DateTime.UtcNow.AddMinutes(-20))
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(abandonedGames);

        // Act
        await InvokeCleanupMethod();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Limpiadas 2 partidas abandonadas")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Invoca el método privado CleanupAbandonedGamesAsync usando reflection
    /// </summary>
    private async Task InvokeCleanupMethod()
    {
        var method = typeof(GameCleanupService).GetMethod(
            "CleanupAbandonedGamesAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var task = (Task)method!.Invoke(_service, null)!;
        await task;
    }

    /// <summary>
    /// Crea una partida de prueba
    /// </summary>
    private static Game CreateGame(int gameId, GameStatus status, DateTime createdAt)
    {
        return new Game
        {
            Id = gameId,
            Status = status,
            CreatedAt = createdAt,
            Name = $"Test Game {gameId}",
            Players = new List<Player>
            {
                new Player
                {
                    Id = gameId * 10,
                    Name = $"Player {gameId}",
                    Uid = $"uid-{gameId}",
                    ConnectionId = $"conn-{gameId}"
                }
            },
            Questions = new List<Question>(),
            MaxQuestions = 15,
            ConditionToWin = 10,
            ExpectedResult = "MAYOR",
            PowerUpsEnabled = true
        };
    }

    #endregion
}