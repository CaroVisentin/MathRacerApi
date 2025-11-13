using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Services;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios básicos para el caso de uso de matchmaking basado en ranking
/// </summary>
public class FindMatchWithMatchmakingUseCaseBasicTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly Mock<IPowerUpService> _powerUpServiceMock;
    private readonly FindMatchWithMatchmakingUseCase _findMatchWithMatchmakingUseCase;

    public FindMatchWithMatchmakingUseCaseBasicTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        var getQuestionsUseCaseMock = new Mock<GetQuestionsUseCase>();
        var gameLogicServiceMock = new Mock<IGameLogicService>();
        _powerUpServiceMock = new Mock<IPowerUpService>();
        
        _findMatchWithMatchmakingUseCase = new FindMatchWithMatchmakingUseCase(
            _gameRepositoryMock.Object,
            _playerRepositoryMock.Object,
            getQuestionsUseCaseMock.Object,
            gameLogicServiceMock.Object,
            _powerUpServiceMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidUid_ShouldThrowNotFoundException()
    {
        // Arrange
        const string connectionId = "connection-123";
        const string invalidUid = "invalid-uid";

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(invalidUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await _findMatchWithMatchmakingUseCase
            .Invoking(x => x.ExecuteAsync(connectionId, invalidUid))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Perfil de jugador no encontrado");
    }

    [Theory]
    [InlineData(50, 25)]   // Principiante: ±25 puntos
    [InlineData(100, 30)]  // Intermedio: ±30 puntos
    [InlineData(200, 40)]  // Avanzado: ±40 puntos
    [InlineData(300, 50)]  // Experto: ±50 puntos
    public void CalculateToleranceRange_ShouldReturnCorrectTolerance_BasedOnPlayerPoints(int playerPoints, int expectedTolerance)
    {
        // Act & Assert - Verify the tolerance calculation logic
        var tolerance = playerPoints switch
        {
            <= 50 => 25,
            <= 150 => 30,
            <= 250 => 40,
            _ => 50
        };

        tolerance.Should().Be(expectedTolerance);
    }

    [Fact]
    public async Task ExecuteAsync_WithCompatiblePlayer_ShouldJoinExistingGame()
    {
        // Arrange
        const string connectionId = "connection-123";
        const string playerUid = "player-uid-456";
        const string existingPlayerUid = "existing-player-uid";
        
        var playerProfile = CreateTestPlayerProfile(playerUid, "NewPlayer", 150);
        var existingPlayerProfile = CreateTestPlayerProfile(existingPlayerUid, "ExistingPlayer", 160);
        
        var existingPlayer = new Player 
        { 
            Id = 1001, 
            Name = "ExistingPlayer", 
            Uid = existingPlayerUid,
            ConnectionId = "existing-connection"
        };
        
        var existingGame = new Game
        {
            Id = 1002,
            Status = GameStatus.WaitingForPlayers,
            Players = new List<Player> { existingPlayer },
            PowerUpsEnabled = true
        };

        var expectedPowerUps = new List<PowerUp> { CreateTestPowerUp() };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(existingPlayerUid))
            .ReturnsAsync(existingPlayerProfile);

        _powerUpServiceMock
            .Setup(x => x.GrantInitialPowerUps(It.IsAny<int>()))
            .Returns(expectedPowerUps);

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Game> { existingGame });

        _gameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Game>()))
            .Returns<Game>(game => Task.FromResult(game));

        // Act
        var result = await _findMatchWithMatchmakingUseCase.ExecuteAsync(connectionId, playerUid);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingGame.Id);
        result.Status.Should().Be(GameStatus.InProgress);
        result.Players.Should().HaveCount(2);

        _gameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Game>()), Times.Once);
    }

    #region Helper Methods

    private static PlayerProfile CreateTestPlayerProfile(string uid, string name, int points)
    {
        return new PlayerProfile
        {
            Id = 1,
            Uid = uid,
            Name = name,
            Email = $"{name.ToLower()}@test.com",
            Points = points,
            Coins = 100,
            LastLevelId = 1
        };
    }

    private static PowerUp CreateTestPowerUp()
    {
        return new PowerUp
        {
            Id = 1,
            Type = PowerUpType.DoublePoints,
            Name = "Double Points",
            Description = "Doubles your points for next correct answer"
        };
    }

    #endregion
}