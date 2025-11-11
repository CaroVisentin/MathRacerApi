using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para CreateCustomOnlineGameUseCase
/// </summary>
public class CreateCustomOnlineGameUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IWorldRepository> _worldRepositoryMock;
    private readonly Mock<ILevelRepository> _levelRepositoryMock;
    private readonly GetQuestionsUseCase _getQuestionsUseCase; 
    private readonly Mock<IPowerUpService> _powerUpServiceMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly CreateCustomOnlineGameUseCase _useCase;

    public CreateCustomOnlineGameUseCaseTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _worldRepositoryMock = new Mock<IWorldRepository>();
        _levelRepositoryMock = new Mock<ILevelRepository>();
        _getQuestionsUseCase = new GetQuestionsUseCase(); 
        _powerUpServiceMock = new Mock<IPowerUpService>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();

        _useCase = new CreateCustomOnlineGameUseCase(
            _gameRepositoryMock.Object,
            _worldRepositoryMock.Object,
            _levelRepositoryMock.Object,
            _getQuestionsUseCase, 
            _powerUpServiceMock.Object,
            _playerRepositoryMock.Object
        );
    }

    #region Validación de Contraseñas

    [Fact]
    public async Task ExecuteAsync_PrivateGameWithoutPassword_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: true,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Las partidas privadas requieren una contraseña.");
    }

    [Fact]
    public async Task ExecuteAsync_PrivateGameWithEmptyPassword_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: true,
            password: "   ",
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Las partidas privadas requieren una contraseña.");
    }

    [Fact]
    public async Task ExecuteAsync_PublicGameWithoutPassword_ShouldSucceed()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Public Game";
        var connectionId = "conn-123";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        result.Should().NotBeNull();
        result.IsPrivate.Should().BeFalse();
        result.Password.Should().BeNull();
    }

    #endregion

    #region Validación de Nombre de Partida

    [Fact]
    public async Task ExecuteAsync_EmptyGameName_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var connectionId = "conn-123";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            "",
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("El nombre de la partida es requerido.");
    }

    [Fact]
    public async Task ExecuteAsync_NullGameName_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var connectionId = "conn-123";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            null!,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("El nombre de la partida es requerido.");
    }

    #endregion

    #region Validación de Jugador

    [Fact]
    public async Task ExecuteAsync_PlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var firebaseUid = "non-existent-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Player*");
    }

    #endregion

    #region Validación de Partidas Activas

    [Fact]
    public async Task ExecuteAsync_PlayerWithActiveGame_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var playerId = 1;
        var gameName = "New Game";
        var connectionId = "conn-123";
        
        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Name = "Test Player",
            Uid = firebaseUid,
            Email = "test@example.com"
        };

        var existingGame = new Game
        {
            Id = 1000,
            Status = GameStatus.InProgress,
            Players = new List<Player>
            {
                new Player { Id = playerId, Name = "Test Player" }
            }
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(playerProfile);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { existingGame });

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Ya tienes una partida activa*");
    }

    [Fact]
    public async Task ExecuteAsync_PlayerWithWaitingGame_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var playerId = 1;
        var gameName = "New Game";
        var connectionId = "conn-123";
        
        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Name = "Test Player",
            Uid = firebaseUid,
            Email = "test@example.com"
        };

        var waitingGame = new Game
        {
            Id = 1000,
            Status = GameStatus.WaitingForPlayers,
            Players = new List<Player>
            {
                new Player { Id = playerId, Name = "Test Player" }
            }
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(playerProfile);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { waitingGame });

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Ya tienes una partida activa*");
    }

    [Fact]
    public async Task ExecuteAsync_PlayerWithFinishedGame_ShouldAllowNewGame()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var playerId = 1;
        var gameName = "New Game";
        var connectionId = "conn-123";
        
        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Name = "Test Player",
            Uid = firebaseUid,
            Email = "test@example.com"
        };

        var finishedGame = new Game
        {
            Id = 1000,
            Status = GameStatus.Finished,
            Players = new List<Player>
            {
                new Player { Id = playerId, Name = "Test Player" }
            }
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(playerProfile);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { finishedGame });

        SetupSuccessfulGameCreation(firebaseUid, playerProfile);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(1);
        result.Players[0].Id.Should().Be(playerId);
    }

    #endregion

    #region Validación de Dificultad

    [Theory]
    [InlineData("INVALIDO")]
    [InlineData("EASY")]
    [InlineData("HARD")]
    [InlineData("")]
    public async Task ExecuteAsync_InvalidDifficulty_ShouldThrowBusinessException(string difficulty)
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        SetupPlayerProfile(firebaseUid);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: difficulty,
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("La dificultad debe ser FACIL, MEDIO o DIFICIL.");
    }

    [Theory]
    [InlineData("FACIL")]
    [InlineData("MEDIO")]
    [InlineData("DIFICIL")]
    [InlineData("facil")]
    [InlineData("medio")]
    [InlineData("dificil")]
    public async Task ExecuteAsync_ValidDifficulty_ShouldSucceed(string difficulty)
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: difficulty,
            expectedResult: "MAYOR"
        );

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Validación de Resultado Esperado

    [Theory]
    [InlineData("INVALIDO")]
    [InlineData("GREATER")]
    [InlineData("LESS")]
    [InlineData("")]
    public async Task ExecuteAsync_InvalidExpectedResult_ShouldThrowBusinessException(string expectedResult)
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        SetupPlayerProfile(firebaseUid);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: expectedResult
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("El resultado esperado debe ser MAYOR o MENOR.");
    }

    [Theory]
    [InlineData("MAYOR")]
    [InlineData("MENOR")]
    [InlineData("mayor")]
    [InlineData("menor")]
    public async Task ExecuteAsync_ValidExpectedResult_ShouldSucceed(string expectedResult)
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: expectedResult
        );

        // Assert
        result.Should().NotBeNull();
        result.ExpectedResult.Should().Be(expectedResult.ToUpperInvariant());
    }

    #endregion

    #region Validación de Mundos y Niveles

    [Fact]
    public async Task ExecuteAsync_NoWorldsForDifficulty_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        SetupPlayerProfile(firebaseUid);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<World>
            {
                new World { Id = 1, Name = "World 1", Difficulty = "MEDIO" }
            });

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*No se encontraron mundos con dificultad FACIL*");
    }

    [Fact]
    public async Task ExecuteAsync_WorldWithoutLevels_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        SetupPlayerProfile(firebaseUid);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<World>
            {
                new World { Id = 1, Name = "Empty World", Difficulty = "FACIL" }
            });

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(1))
            .ReturnsAsync(new List<Level>());

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*no tiene niveles configurados*");
    }

    [Fact]
    public async Task ExecuteAsync_LevelNotFoundForDifficulty_ShouldThrowBusinessException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";
        var connectionId = "conn-123";

        SetupPlayerProfile(firebaseUid);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<World>
            {
                new World { Id = 1, Name = "World 1", Difficulty = "FACIL" }
            });

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(1))
            .ReturnsAsync(new List<Level>
            {
                new Level { Id = 1, Number = 5, WorldId = 1 }
            });

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*No se encontró el nivel 1*");
    }

    #endregion

    #region Creación Exitosa

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldCreateGameSuccessfully()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Epic Battle";
        var connectionId = "conn-123";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid,
            gameName,
            connectionId,
            isPrivate: false,
            password: null,
            difficulty: "MEDIO",
            expectedResult: "MAYOR"
        );

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(gameName);
        result.Status.Should().Be(GameStatus.WaitingForPlayers);
        result.PowerUpsEnabled.Should().BeTrue();
        result.MaxPowerUpsPerPlayer.Should().Be(3);
        result.ExpectedResult.Should().Be("MAYOR");
        result.Players.Should().HaveCount(1);
        result.Questions.Should().HaveCount(40);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGrantInitialPowerUps()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var playerId = 1;

        var powerUps = new List<PowerUp>
        {
            new PowerUp { Id = 1, Type = PowerUpType.DoublePoints },
            new PowerUp { Id = 2, Type = PowerUpType.ShuffleRival }
        };

        SetupSuccessfulGameCreation(firebaseUid);

        _powerUpServiceMock
            .Setup(x => x.GrantInitialPowerUps(playerId))
            .Returns(powerUps);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid,
            "Test Game",
            "conn-123",
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        result.Players[0].AvailablePowerUps.Should().HaveCount(2);
        _powerUpServiceMock.Verify(x => x.GrantInitialPowerUps(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetCreatorPlayerId()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var playerId = 1;

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid,
            "Test Game",
            "conn-123",
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        result.CreatorPlayerId.Should().Be(playerId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateQuestions()
    {
        // Arrange
        var firebaseUid = "test-uid";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid,
            "Test Game",
            "conn-123",
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        result.Questions.Should().HaveCount(40);
        result.Questions.Should().OnlyContain(q => !string.IsNullOrEmpty(q.Equation));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveGameToRepository()
    {
        // Arrange
        var firebaseUid = "test-uid";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        await _useCase.ExecuteAsync(
            firebaseUid,
            "Test Game",
            "conn-123",
            isPrivate: false,
            password: null,
            difficulty: "FACIL",
            expectedResult: "MAYOR"
        );

        // Assert
        _gameRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Game>(g => 
                g.Name == "Test Game" && 
                g.Status == GameStatus.WaitingForPlayers
            )),
            Times.Once
        );
    }

    #endregion

    #region Helper Methods

    private void SetupPlayerProfile(string firebaseUid, int playerId = 1)
    {
        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Name = "Test Player",
            Uid = firebaseUid,
            Email = "test@example.com"
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(playerProfile);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game>());
    }

    private void SetupSuccessfulGameCreation(string firebaseUid, PlayerProfile? profile = null, int playerId = 1)
    {
        if (profile == null)
        {
            profile = new PlayerProfile
            {
                Id = playerId,
                Name = "Test Player",
                Uid = firebaseUid,
                Email = "test@example.com"
            };
        }

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(profile);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game>());

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<World>
            {
                new World 
                { 
                    Id = 1, 
                    Name = "Test World", 
                    Difficulty = "FACIL",
                    OptionsCount = 4,
                    OptionRangeMin = -10,
                    OptionRangeMax = 10,
                    NumberRangeMin = -10,
                    NumberRangeMax = 10,
                    TimePerEquation = 10,
                    Operations = new List<string> { "+", "-" }
                },
                new World 
                { 
                    Id = 2, 
                    Name = "Medium World", 
                    Difficulty = "MEDIO",
                    OptionsCount = 4,
                    OptionRangeMin = -20,
                    OptionRangeMax = 20,
                    NumberRangeMin = -20,
                    NumberRangeMax = 20,
                    TimePerEquation = 8,
                    Operations = new List<string> { "+", "-", "*" }
                },
                new World 
                { 
                    Id = 3, 
                    Name = "Hard World", 
                    Difficulty = "DIFICIL",
                    OptionsCount = 4,
                    OptionRangeMin = -50,
                    OptionRangeMax = 50,
                    NumberRangeMin = -50,
                    NumberRangeMax = 50,
                    TimePerEquation = 6,
                    Operations = new List<string> { "+", "-", "*", "/" }
                }
            });

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int worldId) => new List<Level>
            {
                new Level { Id = 1, Number = 1, WorldId = worldId, TermsCount = 2, VariablesCount = 1 },
                new Level { Id = 6, Number = 6, WorldId = worldId, TermsCount = 3, VariablesCount = 2 },
                new Level { Id = 11, Number = 11, WorldId = worldId, TermsCount = 4, VariablesCount = 3 }
            });

        _powerUpServiceMock
            .Setup(x => x.GrantInitialPowerUps(It.IsAny<int>()))
            .Returns(new List<PowerUp>
            {
                new PowerUp { Id = 1, Type = PowerUpType.DoublePoints, Name = "Puntos Dobles" },
                new PowerUp { Id = 2, Type = PowerUpType.ShuffleRival, Name = "Confundir Rival" }
            });

        _gameRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game g) => g);
    }

    #endregion
}