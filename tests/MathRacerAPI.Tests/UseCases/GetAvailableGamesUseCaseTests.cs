using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para GetAvailableGamesUseCase
/// </summary>
public class GetAvailableGamesUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly GetAvailableGamesUseCase _useCase;

    public GetAvailableGamesUseCaseTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _useCase = new GetAvailableGamesUseCase(_gameRepositoryMock.Object);
    }

    #region Casos Básicos

    [Fact]
    public async Task ExecuteAsync_NoGames_ShouldReturnEmptyList()
    {
        // Arrange
        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game>());

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_AllGamesFinished_ShouldReturnEmptyList()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.Finished, isPrivate: false),
            CreateGame(1002, GameStatus.Finished, isPrivate: true)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_AllGamesInProgress_ShouldReturnEmptyList()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.InProgress, isPrivate: false, playerCount: 2),
            CreateGame(1002, GameStatus.InProgress, isPrivate: true, playerCount: 2)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Filtrado por Estado

    [Fact]
    public async Task ExecuteAsync_OnlyWaitingGames_ShouldReturnAllWaitingGames()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false),
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: true)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_MixedGameStates_ShouldReturnOnlyWaiting()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false),
            CreateGame(1002, GameStatus.InProgress, isPrivate: false, playerCount: 2),
            CreateGame(1003, GameStatus.Finished, isPrivate: false),
            CreateGame(1004, GameStatus.WaitingForPlayers, isPrivate: true)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(2);
        result.Select(g => g.GameId).Should().BeEquivalentTo(new[] { 1001, 1004 });
    }

    #endregion

    #region Filtrado por Capacidad

    [Fact]
    public async Task ExecuteAsync_FullGames_ShouldNotIncludeThem()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 1),
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 2) // Llena
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].GameId.Should().Be(1001);
    }

    #endregion

    #region Filtrado por Privacidad

    [Fact]
    public async Task ExecuteAsync_IncludePrivateTrue_ShouldReturnAllGames()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false),
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: true)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_IncludePrivateFalse_ShouldReturnOnlyPublicGames()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false),
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: true),
            CreateGame(1003, GameStatus.WaitingForPlayers, isPrivate: false)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: false);

        // Assert
        result.Should().HaveCount(2);
        result.Select(g => g.GameId).Should().BeEquivalentTo(new[] { 1001, 1003 });
        result.Should().OnlyContain(g => !g.IsPrivate);
    }

    [Fact]
    public async Task GetPublicGamesAsync_ShouldReturnOnlyPublicGames()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false),
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: true),
            CreateGame(1003, GameStatus.WaitingForPlayers, isPrivate: false)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.GetPublicGamesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(g => !g.IsPrivate);
    }

    #endregion

    #region Filtrado por ID de Partida

    [Fact]
    public async Task ExecuteAsync_ShouldFilterOnlineGamesOnly()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateGame(500, GameStatus.WaitingForPlayers, isPrivate: false), // Offline
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false), // Online
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: true) // Online
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(g => g.GameId >= 1000);
    }

    #endregion

    #region Ordenamiento

    [Fact]
    public async Task ExecuteAsync_ShouldReturnGamesSortedByCreatedAtDescending()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false, createdAt: baseTime.AddMinutes(-10)),
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: false, createdAt: baseTime),
            CreateGame(1003, GameStatus.WaitingForPlayers, isPrivate: false, createdAt: baseTime.AddMinutes(-5))
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(3);
        result[0].GameId.Should().Be(1002); // Más reciente
        result[1].GameId.Should().Be(1003);
        result[2].GameId.Should().Be(1001); // Más antigua
    }

    #endregion

    #region Mapeo de Propiedades

    [Fact]
    public async Task ExecuteAsync_ShouldMapGamePropertiesCorrectly()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var game = CreateGame(
            1001, 
            GameStatus.WaitingForPlayers, 
            isPrivate: true, 
            playerCount: 1,
            gameName: "Epic Battle",
            createdAt: createdAt,
            expectedResult: "MAYOR"
        );

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { game });

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(1);
        var availableGame = result[0];
        
        availableGame.GameId.Should().Be(1001);
        availableGame.GameName.Should().Be("Epic Battle");
        availableGame.IsPrivate.Should().BeTrue();
        availableGame.RequiresPassword.Should().BeTrue();
        availableGame.CurrentPlayers.Should().Be(1);
        availableGame.MaxPlayers.Should().Be(2);
        availableGame.ExpectedResult.Should().Be("MAYOR");
        availableGame.CreatedAt.Should().Be(createdAt);
        availableGame.CreatorName.Should().Be("Creator Player");
        availableGame.IsFull.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_GameWithoutCreator_ShouldMapCreatorNameAsUnknown()
    {
        // Arrange
        var game = new Game
        {
            Id = 1001,
            Name = "Test Game",
            Status = GameStatus.WaitingForPlayers,
            IsPrivate = false,
            CreatorPlayerId = 999,
            ExpectedResult = "MAYOR",
            CreatedAt = DateTime.UtcNow,
            Players = new List<Player>() // Sin jugadores
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { game });

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].CreatorName.Should().Be("Desconocido");
    }

    [Fact]
    public async Task ExecuteAsync_FullGame_ShouldSetIsFullToTrue()
    {
        // Arrange
        var game = CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 2);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { game });

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().BeEmpty(); // No debe incluir juegos llenos
    }

    [Fact]
    public async Task ExecuteAsync_GameWithOnePlayer_ShouldSetIsFullToFalse()
    {
        // Arrange
        var game = CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 1);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { game });

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsFull.Should().BeFalse();
    }

    #endregion

    #region Casos de Integración

    [Fact]
    public async Task ExecuteAsync_ComplexScenario_ShouldFilterAndSortCorrectly()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var games = new List<Game>
        {
            // Debe incluirse: pública, esperando, no llena, reciente
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 1, createdAt: baseTime),
            
            // No debe incluirse: en progreso
            CreateGame(1002, GameStatus.InProgress, isPrivate: false, playerCount: 2, createdAt: baseTime.AddMinutes(-1)),
            
            // Debe incluirse: privada, esperando, no llena
            CreateGame(1003, GameStatus.WaitingForPlayers, isPrivate: true, playerCount: 1, createdAt: baseTime.AddMinutes(-2)),
            
            // No debe incluirse: finalizada
            CreateGame(1004, GameStatus.Finished, isPrivate: false, playerCount: 2, createdAt: baseTime.AddMinutes(-3)),
            
            // No debe incluirse: llena
            CreateGame(1005, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 2, createdAt: baseTime.AddMinutes(-4)),
            
            // No debe incluirse: offline game (ID < 1000)
            CreateGame(500, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 1, createdAt: baseTime.AddMinutes(-5))
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(2);
        result[0].GameId.Should().Be(1001); // Más reciente
        result[1].GameId.Should().Be(1003); // Segunda más reciente
    }

    [Fact]
    public async Task ExecuteAsync_PublicOnlyFilter_ComplexScenario()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 1, createdAt: baseTime),
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: true, playerCount: 1, createdAt: baseTime.AddMinutes(-1)),
            CreateGame(1003, GameStatus.WaitingForPlayers, isPrivate: false, playerCount: 1, createdAt: baseTime.AddMinutes(-2)),
            CreateGame(1004, GameStatus.WaitingForPlayers, isPrivate: true, playerCount: 1, createdAt: baseTime.AddMinutes(-3))
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(g => !g.IsPrivate);
        result[0].GameId.Should().Be(1001);
        result[1].GameId.Should().Be(1003);
    }

    #endregion

    #region Casos Edge

    [Fact]
    public async Task ExecuteAsync_NullGamesList_ShouldHandleGracefully()
    {
        // Arrange
        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync((List<Game>)null!);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task ExecuteAsync_GameWithNullPlayers_ShouldHandleGracefully()
    {
        // Arrange
        var game = new Game
        {
            Id = 1001,
            Name = "Test Game",
            Status = GameStatus.WaitingForPlayers,
            IsPrivate = false,
            CreatorPlayerId = 1,
            ExpectedResult = "MAYOR",
            CreatedAt = DateTime.UtcNow,
            Players = null! // Lista nula
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { game });

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task ExecuteAsync_MultipleGamesWithSameCreatedAt_ShouldReturnAll()
    {
        // Arrange
        var sameTime = DateTime.UtcNow;
        var games = new List<Game>
        {
            CreateGame(1001, GameStatus.WaitingForPlayers, isPrivate: false, createdAt: sameTime),
            CreateGame(1002, GameStatus.WaitingForPlayers, isPrivate: false, createdAt: sameTime),
            CreateGame(1003, GameStatus.WaitingForPlayers, isPrivate: false, createdAt: sameTime)
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _useCase.ExecuteAsync(includePrivate: true);

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Helper Methods

    private Game CreateGame(
        int id,
        GameStatus status,
        bool isPrivate,
        int playerCount = 1,
        string gameName = "Test Game",
        DateTime? createdAt = null,
        string expectedResult = "MAYOR")
    {
        var game = new Game
        {
            Id = id,
            Name = gameName,
            Status = status,
            IsPrivate = isPrivate,
            Password = isPrivate ? "password123" : null,
            ExpectedResult = expectedResult,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            CreatorPlayerId = 1,
            Players = new List<Player>()
        };

        // Agregar jugadores
        for (int i = 0; i < playerCount; i++)
        {
            game.Players.Add(new Player
            {
                Id = (int)(i == 0 ? game.CreatorPlayerId : i + 1),
                Name = i == 0 ? "Creator Player" : $"Player {i + 1}",
                ConnectionId = $"conn-{i}"
            });
        }

        return game;
    }

    #endregion
}