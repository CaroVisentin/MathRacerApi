using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de obtención de mundos
/// </summary>
public class GetWorldsUseCaseTests
{
    private readonly Mock<IWorldRepository> _worldRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;
    private readonly GetWorldsUseCase _getWorldsUseCase;

    public GetWorldsUseCaseTests()
    {
        _worldRepositoryMock = new Mock<IWorldRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        
        _getPlayerByIdUseCase = new GetPlayerByIdUseCase(_playerRepositoryMock.Object);
        
        _getWorldsUseCase = new GetWorldsUseCase(
            _worldRepositoryMock.Object,
            _getPlayerByIdUseCase);
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerIsNew_ShouldReturnAllWorldsWithWorld1Available()
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 0);
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(0))
            .ReturnsAsync(1); // Jugador nuevo = Mundo 1

        // Act
        var result = await _getWorldsUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Worlds.Should().HaveCount(3);
        result.LastAvailableWorldId.Should().Be(1);
        
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetWorldIdByLevelIdAsync(0), Times.Once);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerCompletedWorld1_ShouldReturnWorld2Available()
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 10); // Completó mundo 1
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(10))
            .ReturnsAsync(2); // Level 10 pertenece al mundo 2

        // Act
        var result = await _getWorldsUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Worlds.Should().HaveCount(3);
        result.LastAvailableWorldId.Should().Be(2);
        result.Worlds.Should().ContainSingle(w => w.Id == 2);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerCompletedAllWorlds_ShouldReturnLastWorld()
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 30); // Completó mundo 3
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(30))
            .ReturnsAsync(3); // Level 30 pertenece al mundo 3

        // Act
        var result = await _getWorldsUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Worlds.Should().HaveCount(3);
        result.LastAvailableWorldId.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldReturnWorldsInCorrectOrder()
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1);

        // Act
        var result = await _getWorldsUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Worlds.Should().BeInAscendingOrder(w => w.Id);
        result.Worlds[0].Id.Should().Be(1);
        result.Worlds[1].Id.Should().Be(2);
        result.Worlds[2].Id.Should().Be(3);
    }

    #endregion

    #region Validation Tests - Player Not Found

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const string uid = "non-existent-uid";

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _getWorldsUseCase.ExecuteByUidAsync(uid));

        exception.Message.Should().Contain("No se encontró un jugador con el UID proporcionado.");
        
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Never);
    }

    #endregion

    #region Validation Tests - No Worlds

    [Fact]
    public async Task ExecuteByUidAsync_WhenNoWorldsExist_ShouldThrowBusinessException()
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 5);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ThrowsAsync(new BusinessException("No hay mundos configurados en el sistema."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _getWorldsUseCase.ExecuteByUidAsync(uid));

        exception.Message.Should().Contain("No hay mundos configurados");
        
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Once);
    }

    #endregion

    #region Validation Tests - Invalid World

    [Fact]
    public async Task ExecuteByUidAsync_WhenLastAvailableWorldDoesNotExist_ShouldThrowBusinessException()
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 999);
        var worlds = CreateTestWorlds(); // Solo mundos 1, 2, 3

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(999))
            .ReturnsAsync(99); // Mundo que no existe en la lista

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _getWorldsUseCase.ExecuteByUidAsync(uid));

        exception.Message.Should().Contain("El mundo con ID 99 no existe");
        
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetWorldIdByLevelIdAsync(999), Times.Once);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteByUidAsync_ShouldCallRepositoriesInCorrectOrder()
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds();
        var callOrder = new List<string>();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .Callback(() => callOrder.Add("GetPlayer"))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .Callback(() => callOrder.Add("GetWorlds"))
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .Callback(() => callOrder.Add("GetWorldId"))
            .ReturnsAsync(1);

        // Act
        await _getWorldsUseCase.ExecuteByUidAsync(uid);

        // Assert
        callOrder.Should().ContainInOrder("GetPlayer", "GetWorlds", "GetWorldId");
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldCallGetWorldIdByLevelIdWithCorrectParameter()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int lastLevelId = 15;
        var player = CreateTestPlayer(uid, lastLevelId);
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(lastLevelId))
            .ReturnsAsync(2);

        // Act
        await _getWorldsUseCase.ExecuteByUidAsync(uid);

        // Assert
        _worldRepositoryMock.Verify(
            x => x.GetWorldIdByLevelIdAsync(lastLevelId), 
            Times.Once);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(0, 1)]   // Jugador nuevo
    [InlineData(5, 1)]   // Mundo 1
    [InlineData(15, 2)]  // Mundo 2
    [InlineData(25, 3)]  // Mundo 3
    public async Task ExecuteByUidAsync_WithDifferentLevelIds_ShouldReturnCorrectWorld(
        int lastLevelId, 
        int expectedWorldId)
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId);
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(lastLevelId))
            .ReturnsAsync(expectedWorldId);

        // Act
        var result = await _getWorldsUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.LastAvailableWorldId.Should().Be(expectedWorldId);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WithMultiplePlayers_ShouldReturnCorrectDataForEach()
    {
        // Arrange
        const string uid1 = "player-1";
        const string uid2 = "player-2";
        
        var player1 = CreateTestPlayer(uid1, lastLevelId: 5);
        var player2 = CreateTestPlayer(uid2, lastLevelId: 20);
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid1))
            .ReturnsAsync(player1);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid2))
            .ReturnsAsync(player2);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(20))
            .ReturnsAsync(3);

        // Act
        var result1 = await _getWorldsUseCase.ExecuteByUidAsync(uid1);
        var result2 = await _getWorldsUseCase.ExecuteByUidAsync(uid2);

        // Assert
        result1.LastAvailableWorldId.Should().Be(1);
        result2.LastAvailableWorldId.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldReturnWorldsWithAllProperties()
    {
        // Arrange
        const string uid = "test-uid-123";
        var player = CreateTestPlayer(uid, lastLevelId: 5);
        var worlds = CreateTestWorlds();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _worldRepositoryMock
            .Setup(x => x.GetWorldIdByLevelIdAsync(5))
            .ReturnsAsync(1);

        // Act
        var result = await _getWorldsUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Worlds.Should().OnlyContain(w => 
            !string.IsNullOrEmpty(w.Name) &&
            w.OptionsCount > 0 &&
            w.TimePerEquation > 0 &&
            w.Operations.Count > 0);
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

    #endregion
}