using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

public class JoinCreatedGameUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly Mock<IPowerUpService> _powerUpServiceMock;
    private readonly JoinCreatedGameUseCase _useCase;

    public JoinCreatedGameUseCaseTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _powerUpServiceMock = new Mock<IPowerUpService>();

        _useCase = new JoinCreatedGameUseCase(
            _gameRepositoryMock.Object,
            _playerRepositoryMock.Object,
            _powerUpServiceMock.Object
        );
    }

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
    public async Task ExecuteAsync_GameAlreadyInProgress_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.InProgress,
            CreatorPlayerId = 1
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
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*ya comenzó*");
    }

    [Fact]
    public async Task ExecuteAsync_GameFinished_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.Finished,
            CreatorPlayerId = 1
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
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*finalizó*");
    }

    #endregion

    #region Validación de Contraseña

    [Fact]
    public async Task ExecuteAsync_PrivateGameWithoutPassword_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            IsPrivate = true,
            Password = "secret123",
            CreatorPlayerId = 1
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
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*privada*requiere contraseña*");
    }

    [Fact]
    public async Task ExecuteAsync_PrivateGameWithWrongPassword_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 2;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            IsPrivate = true,
            Password = "correct123",
            CreatorPlayerId = 1,
            Players = new List<Player>
            {
                new Player { Id = 1, Name = "Creator" }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Test Player"
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
            password: "wrong123"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
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
            CreatorPlayerId = 1,
            Players = new List<Player>
            {
                new Player { Id = 1, Name = "Creator" }
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
            password: correctPassword
        );

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(2);
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
            Status = GameStatus.WaitingForPlayers,
            CreatorPlayerId = 1
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
            .WithMessage("*Player*");
    }

    [Fact]
    public async Task ExecuteAsync_PlayerTriesToJoinOwnGame_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "creator-uid";
        var creatorId = 1;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            CreatorPlayerId = creatorId,
            Players = new List<Player>
            {
                new Player { Id = creatorId, Name = "Creator" }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = creatorId,
            Uid = firebaseUid,
            Name = "Creator"
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
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*No puedes unirte a tu propia partida*");
    }

    [Fact]
    public async Task ExecuteAsync_PlayerWithAnotherActiveGame_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 2;

        var targetGame = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            CreatorPlayerId = 1,
            Players = new List<Player>
            {
                new Player { Id = 1, Name = "Creator" }
            }
        };

        var activeGame = new Game
        {
            Id = 1002,
            Status = GameStatus.InProgress,
            Players = new List<Player>
            {
                new Player { Id = playerId, Name = "Test Player" }
            }
        };

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Uid = firebaseUid,
            Name = "Test Player"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(targetGame);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { targetGame, activeGame });

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
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Ya tienes una partida activa*");
    }

    #endregion

    #region Capacidad de Partida

    [Fact]
    public async Task ExecuteAsync_GameIsFull_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1001;
        var firebaseUid = "player-uid";
        var playerId = 3;

        var game = new Game
        {
            Id = gameId,
            Status = GameStatus.WaitingForPlayers,
            CreatorPlayerId = 1,
            Players = new List<Player>
            {
                new Player { Id = 1, Name = "Player 1" },
                new Player { Id = 2, Name = "Player 2" }
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

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { game });

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
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*La partida está llena*");
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
                new Player { Id = 1, Name = "Creator" }
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
    public async Task ExecuteAsync_ShouldGrantPowerUpsToJoiningPlayer()
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
                new Player { Id = 1, Name = "Creator" }
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
            .Setup(x => x.GrantInitialPowerUps(playerId))
            .Returns(powerUps);

        // Act
        var result = await _useCase.ExecuteAsync(
            gameId,
            firebaseUid,
            "conn-123",
            password: null
        );

        // Assert
        var joinedPlayer = result.Players.FirstOrDefault(p => p.Id == playerId);
        joinedPlayer.Should().NotBeNull();
        joinedPlayer!.AvailablePowerUps.Should().HaveCount(2);
        _powerUpServiceMock.Verify(x => x.GrantInitialPowerUps(playerId), Times.Once);
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
                new Player { Id = 1, Name = "Creator" }
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
                new Player { Id = 1, Name = "Creator" }
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

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { game });

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(playerProfile);

        _powerUpServiceMock
            .Setup(x => x.GrantInitialPowerUps(playerProfile.Id))
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