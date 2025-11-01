using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de obtención de niveles de un mundo
/// </summary>
public class GetWorldLevelsUseCaseTests
{
    private readonly Mock<ILevelRepository> _levelRepositoryMock;
    private readonly Mock<IWorldRepository> _worldRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;
    private readonly GetWorldLevelsUseCase _getWorldLevelsUseCase;

    public GetWorldLevelsUseCaseTests()
    {
        _levelRepositoryMock = new Mock<ILevelRepository>();
        _worldRepositoryMock = new Mock<IWorldRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();

        _getPlayerByIdUseCase = new GetPlayerByIdUseCase(_playerRepositoryMock.Object);

        _getWorldLevelsUseCase = new GetWorldLevelsUseCase(
            _levelRepositoryMock.Object,
            _worldRepositoryMock.Object,
            _getPlayerByIdUseCase);
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerIsNew_ShouldReturnWorld1Levels()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 1;
        var player = CreateTestPlayer(uid, lastLevelId: 0);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(0))
            .ReturnsAsync(1);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        var result = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        result.Should().NotBeNull();
        result.WorldName.Should().Be("Mundo 1 - Suma y Resta");
        result.Levels.Should().HaveCount(10);
        result.LastCompletedLevelId.Should().Be(0);

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetWorldIdByLevelIdAsync(0), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Once);
        _levelRepositoryMock.Verify(x => x.GetAllByWorldIdAsync(worldId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerCompletedSomeLevels_ShouldReturnCorrectProgress()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 1;
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        var result = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        result.Should().NotBeNull();
        result.LastCompletedLevelId.Should().Be(5);
        result.Levels.Should().HaveCount(10);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerCompletedWorld_ShouldReturnAllLevels()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 1;
        var player = CreateTestPlayer(uid, lastLevelId: 10);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(10))
            .ReturnsAsync(2); // Está en el mundo 2

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        var result = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        result.Should().NotBeNull();
        result.Levels.Should().HaveCount(10);
        result.LastCompletedLevelId.Should().Be(10);
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldReturnLevelsInCorrectOrder()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 1;
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        var result = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        result.Levels.Should().BeInAscendingOrder(l => l.Number);
        result.Levels.First().Number.Should().Be(1);
        result.Levels.Last().Number.Should().Be(10);
    }

    #endregion

    #region Validation Tests - Invalid WorldId

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task ExecuteByUidAsync_WithInvalidWorldId_ShouldThrowValidationException(int invalidWorldId)
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 5);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _getWorldLevelsUseCase.ExecuteByUidAsync(uid, invalidWorldId));

        exception.Message.Should().Contain("El ID del mundo debe ser mayor a 0");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Never);
        _levelRepositoryMock.Verify(x => x.GetAllByWorldIdAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Validation Tests - Player Not Found

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const string uid = "non-existent-uid";
        const int worldId = 1;

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId));

        exception.Message.Should().Contain("No se encontró un jugador con el UID proporcionado.");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Never);
    }

    #endregion

    #region Validation Tests - World Not Unlocked

    [Fact]
    public async Task ExecuteByUidAsync_WhenWorldNotUnlocked_ShouldThrowBusinessException()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int requestedWorldId = 3;
        var player = CreateTestPlayer(uid, lastLevelId: 5);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1); // Jugador está en mundo 1

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _getWorldLevelsUseCase.ExecuteByUidAsync(uid, requestedWorldId));

        exception.Message.Should().Contain("No tienes acceso al mundo 3");
        exception.Message.Should().Contain("Completa los niveles del mundo 1");

        _worldRepositoryMock.Verify(x => x.GetWorldIdByLevelIdAsync(5), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Never);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(1, 3)]
    [InlineData(2, 3)]
    public async Task ExecuteByUidAsync_WhenTryingToAccessFutureWorld_ShouldThrowBusinessException(
        int playerWorldId,
        int requestedWorldId)
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 5);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(playerWorldId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _getWorldLevelsUseCase.ExecuteByUidAsync(uid, requestedWorldId));

        exception.Message.Should().Contain($"No tienes acceso al mundo {requestedWorldId}");
    }

    #endregion

    #region Validation Tests - World Not Found

    [Fact]
    public async Task ExecuteByUidAsync_WhenWorldDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 99;
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds(); // Solo mundos 1, 2, 3

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(99); // Permitir acceso al mundo 99

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId));

        exception.Message.Should().Contain($"Mundo con ID {worldId} no fue encontrado");

        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Once);
    }

    #endregion

    #region Validation Tests - No Levels

    [Fact]
    public async Task ExecuteByUidAsync_WhenWorldHasNoLevels_ShouldThrowBusinessException()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 1;
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(new List<Level>()); // Sin niveles

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId));

        exception.Message.Should().Contain("no tiene niveles configurados");

        _levelRepositoryMock.Verify(x => x.GetAllByWorldIdAsync(worldId), Times.Once);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteByUidAsync_ShouldCallRepositoriesInCorrectOrder()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 1;
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);
        var callOrder = new List<string>();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .Callback(() => callOrder.Add("GetPlayer"))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .Callback(() => callOrder.Add("GetWorldId"))
            .ReturnsAsync(1);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .Callback(() => callOrder.Add("GetWorlds"))
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .Callback(() => callOrder.Add("GetLevels"))
            .ReturnsAsync(levels);

        // Act
        await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        callOrder.Should().ContainInOrder("GetPlayer", "GetWorldId", "GetWorlds", "GetLevels");
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldCallGetWorldIdByLevelIdWithCorrectParameter()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 1;
        const int lastLevelId = 15;
        var player = CreateTestPlayer(uid, lastLevelId);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(lastLevelId))
            .ReturnsAsync(2);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        _worldRepositoryMock.Verify(
            x => x.GetWorldIdByLevelIdAsync(lastLevelId),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldCallGetAllByWorldIdWithCorrectParameter()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 2;
        var player = CreateTestPlayer(uid, lastLevelId: 15);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(15))
            .ReturnsAsync(2);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        _levelRepositoryMock.Verify(
            x => x.GetAllByWorldIdAsync(worldId),
            Times.Once);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(0, 1, 1)]   // Jugador nuevo accediendo a mundo 1
    [InlineData(5, 1, 1)]   // Jugador en mundo 1 accediendo a mundo 1
    [InlineData(15, 2, 1)]  // Jugador en mundo 2 accediendo a mundo 1 (completado)
    [InlineData(15, 2, 2)]  // Jugador en mundo 2 accediendo a mundo 2
    [InlineData(25, 3, 1)]  // Jugador en mundo 3 accediendo a mundo 1
    [InlineData(25, 3, 2)]  // Jugador en mundo 3 accediendo a mundo 2
    [InlineData(25, 3, 3)]  // Jugador en mundo 3 accediendo a mundo 3
    public async Task ExecuteByUidAsync_WithDifferentScenarios_ShouldReturnCorrectData(
        int lastLevelId,
        int playerWorldId,
        int requestedWorldId)
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(requestedWorldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(lastLevelId))
            .ReturnsAsync(playerWorldId);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(requestedWorldId))
            .ReturnsAsync(levels);

        // Act
        var result = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, requestedWorldId);

        // Assert
        result.Should().NotBeNull();
        result.LastCompletedLevelId.Should().Be(lastLevelId);
        result.Levels.Should().HaveCount(10);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WithMultiplePlayers_ShouldReturnCorrectDataForEach()
    {
        // Arrange
        const string uid1 = "player-1";
        const string uid2 = "player-2";
        const int worldId = 1;

        var player1 = CreateTestPlayer(uid1, lastLevelId: 5);
        var player2 = CreateTestPlayer(uid2, lastLevelId: 20);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid1))
            .ReturnsAsync(player1);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid2))
            .ReturnsAsync(player2);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(20))
            .ReturnsAsync(3);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        var result1 = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid1, worldId);
        var result2 = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid2, worldId);

        // Assert
        result1.LastCompletedLevelId.Should().Be(5);
        result2.LastCompletedLevelId.Should().Be(20);
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldReturnLevelsWithAllProperties()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 1;
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        var result = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        result.Levels.Should().OnlyContain(l =>
            l.Id > 0 &&
            l.Number > 0 &&
            l.WorldId == worldId &&
            l.TermsCount > 0 &&
            l.VariablesCount > 0 &&
            !string.IsNullOrEmpty(l.ResultType));
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldReturnCorrectWorldName()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int worldId = 2;
        var player = CreateTestPlayer(uid, lastLevelId: 15);
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels(worldId);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(15))
            .ReturnsAsync(2);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _levelRepositoryMock
            .Setup(x => x.GetAllByWorldIdAsync(worldId))
            .ReturnsAsync(levels);

        // Act
        var result = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

        // Assert
        result.WorldName.Should().Be("Mundo 2 - Multiplicación");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Crea un jugador de prueba
    /// </summary>
    private static PlayerProfile CreateTestPlayer(string uid, int lastLevelId = 0)
    {
        return new PlayerProfile
        {
            Id = 100,
            Uid = uid,
            Name = "TestPlayer",
            Email = "test@test.com",
            LastLevelId = lastLevelId,
            Coins = 100
        };
    }

    /// <summary>
    /// Crea una lista de mundos de prueba
    /// </summary>
    private static List<World> CreateTestWorlds()
    {
        return new List<World>
        {
            new World
            {
                Id = 1,
                Name = "Mundo 1 - Suma y Resta",
                OptionsCount = 4,
                OptionRangeMin = -10,
                OptionRangeMax = 10,
                NumberRangeMin = -10,
                NumberRangeMax = 10,
                TimePerEquation = 10,
                Difficulty = "Fácil",
                Operations = new List<string> { "+", "-" },
                Levels = new List<Level>()
            },
            new World
            {
                Id = 2,
                Name = "Mundo 2 - Multiplicación",
                OptionsCount = 4,
                OptionRangeMin = -20,
                OptionRangeMax = 20,
                NumberRangeMin = -20,
                NumberRangeMax = 20,
                TimePerEquation = 15,
                Difficulty = "Medio",
                Operations = new List<string> { "*" },
                Levels = new List<Level>()
            },
            new World
            {
                Id = 3,
                Name = "Mundo 3 - División",
                OptionsCount = 4,
                OptionRangeMin = -30,
                OptionRangeMax = 30,
                NumberRangeMin = -30,
                NumberRangeMax = 30,
                TimePerEquation = 20,
                Difficulty = "Difícil",
                Operations = new List<string> { "/" },
                Levels = new List<Level>()
            }
        };
    }

    /// <summary>
    /// Crea una lista de niveles de prueba para un mundo específico
    /// </summary>
    private static List<Level> CreateTestLevels(int worldId)
    {
        var levels = new List<Level>();
        for (int i = 1; i <= 10; i++)
        {
            levels.Add(new Level
            {
                Id = (worldId - 1) * 10 + i,
                WorldId = worldId,
                Number = i,
                TermsCount = 2 + (i / 3),
                VariablesCount = 1 + (i / 5),
                ResultType = i % 2 == 0 ? "MAYOR" : "MENOR"
            });
        }
        return levels;
    }

    #endregion
}