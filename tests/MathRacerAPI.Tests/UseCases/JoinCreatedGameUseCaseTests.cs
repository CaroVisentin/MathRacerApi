using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace MathRacerAPI.Tests.UseCases;

public class JoinCreatedGameUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly Mock<IPowerUpService> _powerUpServiceMock;
    private readonly Mock<ILogger<JoinCreatedGameUseCase>> _loggerMock;
    private readonly JoinCreatedGameUseCase _useCase;

    public JoinCreatedGameUseCaseTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _powerUpServiceMock = new Mock<IPowerUpService>();
        _loggerMock = new Mock<ILogger<JoinCreatedGameUseCase>>();

        _useCase = new JoinCreatedGameUseCase(
            _gameRepositoryMock.Object,
            _playerRepositoryMock.Object,
            _powerUpServiceMock.Object,
            _loggerMock.Object
        );
    }

    #region Validación de Entrada

    [Fact]
    public async Task ExecuteAsync_EmptyFirebaseUid_ShouldThrowValidationException()
    {
        // Arrange
        var gameId = 1001;

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            gameId,
            "",
            "conn-123",
            password: null
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*UID*requerido*");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyConnectionId_ShouldThrowValidationException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "test-uid";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "",
            password: null
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*ConnectionId*requerido*");
    }

    #endregion

    #region Validación de Partida

    [Fact]
    public async Task ExecuteAsync_GameNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var gameId = 9999;
        var firebaseUid = "test-uid";

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((Game?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Game*");
    }

    [Fact]
    public async Task ExecuteAsync_GameNotWaitingForPlayers_ShouldThrowValidationException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.InProgress
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*no está disponible*");
    }

    #endregion

    #region Validación de Contraseña

    [Fact]
    public async Task ExecuteAsync_PrivateGameWithWrongPassword_ShouldThrowValidationException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            IsPrivate = true,
            Password = "correct123"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: "wrong123"
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Contraseña incorrecta*");
    }

    [Fact]
    public async Task ExecuteAsync_PrivateGameWithCorrectPassword_ShouldSucceed()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 2;
        var correctPassword = "secret123";

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            IsPrivate = true,
            Password = correctPassword,
            Players = new List<Player>()
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Test Player"
        };

        SetupSuccessfulJoin(gameId, firebaseUid, game, playerProfile);

        // Act
        var result = await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: correctPassword
        );

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(1);
    }

    #endregion

    #region Validación de Jugador

    [Fact]
    public async Task ExecuteAsync_PlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "non-existent-uid";

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*jugador*");
    }

    #endregion

    #region Capacidad de Partida

    [Fact]
    public async Task ExecuteAsync_GameIsFull_ShouldThrowValidationException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 3;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            Players = new List<Player>
            {
                new Player { Id = 1, Uid = "uid1", Name = "Player 1", ConnectionId = "conn1" },
                new Player { Id = 2, Uid = "uid2", Name = "Player 2", ConnectionId = "conn2" }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Player 3"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(playerProfile);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*llena*");
    }

    #endregion

    #region Actualización de ConnectionId

    [Fact]
    public async Task ExecuteAsync_PlayerAlreadyExists_ShouldUpdateConnectionId()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "existing-uid";
        var playerId = 1;
        var oldConnectionId = "old-conn-123";
        var newConnectionId = "new-conn-456";

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            Players = new List<Player>
            {
                new Player 
                { 
                    Id = playerId, 
                    Uid = firebaseUid, 
                    Name = "Existing Player", 
                    ConnectionId = oldConnectionId 
                }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Existing Player"
        };

        SetupSuccessfulJoin(gameId, firebaseUid, game, playerProfile);

        // Act
        var result = await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            newConnectionId,
            password: null
        );

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(1);
        result.Players[0].ConnectionId.Should().Be(newConnectionId);
        result.Players[0].Id.Should().Be(playerId);
    }

    [Fact]
    public async Task ExecuteAsync_FirstPlayerJoining_ShouldSetAsCreator()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "creator-uid";
        var playerId = 1;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            Players = new List<Player>(),
            CreatorPlayerId = null
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Creator"
        };

        SetupSuccessfulJoin(gameId, firebaseUid, game, playerProfile);

        // Act
        var result = await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        result.CreatorPlayerId.Should().Be(playerId);
        result.Players.Should().HaveCount(1);
    }

    #endregion

    #region Unión Exitosa

    [Fact]
    public async Task ExecuteAsync_ValidPlayer_ShouldJoinSuccessfully()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 2;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            CreatorPlayerId = 1,
            Players = new List<Player>
            {
                new Player { Id = 1, Uid = "creator-uid", Name = "Creator", ConnectionId = "conn1" }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Test Player"
        };

        SetupSuccessfulJoin(gameId, firebaseUid, game, playerProfile);

        // Act
        var result = await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(2);
        result.Status.Should().Be(GameStatus.InProgress);
        result.Players.Should().Contain(p => p.Id == playerId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGrantPowerUpsToNewPlayer()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 2;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            CreatorPlayerId = 1,
            Players = new List<Player>
            {
                new Player { Id = 1, Uid = "creator-uid", Name = "Creator", ConnectionId = "conn1" }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Test Player"
        };

        var powerUps = new List<PowerUp>
        {
            new PowerUp { Id = 3, Type = PowerUpType.DoublePoints },
            new PowerUp { Id = 4, Type = PowerUpType.ShuffleRival }
        };

        SetupSuccessfulJoin(gameId, firebaseUid, game, playerProfile);

        _powerUpServiceMock
            .Setup(x => x.GrantInitialPowerUps(It.IsAny<int>()))
            .Returns(powerUps);

        // Act
        var result = await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        var joinedPlayer = result.Players.FirstOrDefault(p => p.Uid == firebaseUid);
        joinedPlayer.Should().NotBeNull();
        joinedPlayer!.AvailablePowerUps.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithTwoPlayers_ShouldChangeStatusToInProgress()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 2;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            CreatorPlayerId = 1,
            Players = new List<Player>
            {
                new Player { Id = 1, Uid = "creator-uid", Name = "Creator", ConnectionId = "conn1" }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Test Player"
        };

        SetupSuccessfulJoin(gameId, firebaseUid, game, playerProfile);

        // Act
        var result = await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        result.Status.Should().Be(GameStatus.InProgress);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateGameInRepository()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 2;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            CreatorPlayerId = 1,
            Players = new List<Player>
            {
                new Player { Id = 1, Uid = "creator-uid", Name = "Creator", ConnectionId = "conn1" }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Test Player"
        };

        SetupSuccessfulJoin(gameId, firebaseUid, game, playerProfile);

        // Act
        await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        _gameRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Game>(g => 
                g.Id == gameId && 
                g.Players.Count == 2 &&
                g.Status == GameStatus.InProgress
            )),
            Times.Once
        );
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulJoin(
        int gameId, 
        string firebaseUid, 
        Game game, 
        PlayerProfile playerProfile)
    {
        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(playerProfile);

        _powerUpServiceMock
            .Setup(x => x.GrantInitialPowerUps(It.IsAny<int>()))
            .Returns(new List<PowerUp>
            {
                new PowerUp { Id = 1, Type = PowerUpType.DoublePoints },
                new PowerUp { Id = 2, Type = PowerUpType.ShuffleRival }
            });

        _gameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);
    }

    #endregion
}