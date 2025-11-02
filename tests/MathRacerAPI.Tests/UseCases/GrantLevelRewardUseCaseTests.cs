using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using System.Threading.Tasks;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de otorgamiento de recompensas por nivel
/// </summary>
public class GrantLevelRewardUseCaseTests
{
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly GrantLevelRewardUseCase _grantLevelRewardUseCase;

    public GrantLevelRewardUseCaseTests()
    {
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _grantLevelRewardUseCase = new GrantLevelRewardUseCase(_playerRepositoryMock.Object);
    }

    #region Player Validation Tests

    [Fact]
    public async Task ExecuteAsync_WhenPlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const int playerId = 999;
        const int levelId = 5;
        const int worldId = 2;

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId));

        exception.Message.Should().Contain("No se encontró un jugador");
        
        _playerRepositoryMock.Verify(x => x.GetByIdAsync(playerId), Times.Once);
        _playerRepositoryMock.Verify(x => x.AddCoinsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region First Completion Tests

    [Fact]
    public async Task ExecuteAsync_WhenFirstCompletion_ShouldGrantFullReward()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 5;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 4); // levelId 5 > lastLevelId 4

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(playerId, levelId))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByIdAsync(playerId), Times.Once);
        
        // Verificar que se otorgaron monedas (worldId * 100 ± 20%)
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => 
                coins >= 160 && coins <= 240)), // 200 ± 20% = [160, 240]
            Times.Once);

        // Verificar que se actualizó el LastLevelId
        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(playerId, levelId), Times.Once);
    }

    [Theory]
    [InlineData(1, 80, 120)]    // Mundo 1: 100 ± 20% = [80, 120]
    [InlineData(2, 160, 240)]   // Mundo 2: 200 ± 20% = [160, 240]
    [InlineData(3, 240, 360)]   // Mundo 3: 300 ± 20% = [240, 360]
    [InlineData(5, 400, 600)]   // Mundo 5: 500 ± 20% = [400, 600]
    public async Task ExecuteAsync_WhenFirstCompletion_ShouldRespectWorldMultiplier(
        int worldId, int minCoins, int maxCoins)
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 10;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 5);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(playerId, levelId))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => 
                coins >= minCoins && coins <= maxCoins)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFirstCompletion_ShouldUpdateLastLevelId()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 8;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 5);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(playerId, levelId))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(playerId, levelId), Times.Once);
    }

    #endregion

    #region Repeated Completion Tests

    [Fact]
    public async Task ExecuteAsync_WhenRepeatedCompletion_ShouldGrantReducedReward()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 5;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 10); // levelId 5 <= lastLevelId 10

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByIdAsync(playerId), Times.Once);
        
        // Verificar que se otorgaron monedas reducidas (worldId * 10 ± 1%)
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => 
                coins >= 18 && coins <= 22)), // 20 ± 1% = [19.8, 20.2] ≈ [18, 22]
            Times.Once);

        // Verificar que NO se actualizó el LastLevelId
        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(1, 9, 11)]     // Mundo 1: 10 ± 1% ≈ [9, 11]
    [InlineData(2, 18, 22)]    // Mundo 2: 20 ± 1% ≈ [18, 22]
    [InlineData(3, 27, 33)]    // Mundo 3: 30 ± 1% ≈ [27, 33]
    [InlineData(5, 45, 55)]    // Mundo 5: 50 ± 1% ≈ [45, 55]
    public async Task ExecuteAsync_WhenRepeatedCompletion_ShouldRespectWorldMultiplier(
        int worldId, int minCoins, int maxCoins)
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 5;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 20); // Nivel repetido

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => 
                coins >= minCoins && coins <= maxCoins)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepeatedCompletion_ShouldNotUpdateLastLevelId()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 5;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 10);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenReplayingSameLevel_ShouldGrantReducedReward()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 5;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 5); // Mismo nivel

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        // Debe ser recompensa reducida porque levelId (5) NO es > lastLevelId (5)
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => 
                coins >= 18 && coins <= 22)), 
            Times.Once);
        
        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ExecuteAsync_WhenNewPlayer_ShouldGrantFullReward()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 1;
        const int worldId = 1;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 0); // Jugador nuevo (nivel inicial 0)

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(playerId, levelId))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        // Como levelId (1) > lastLevelId (0), ES primera vez → recompensa COMPLETA
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => 
                coins >= 80 && coins <= 120)), // Mundo 1: 100 ± 20% = [80, 120]
            Times.Once);
    
        // Debe actualizar LastLevelId porque es primera vez
        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(playerId, levelId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCompletingNextLevel_ShouldGrantFullReward()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 6;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 5); // Siguiente nivel

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(playerId, levelId))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => 
                coins >= 160 && coins <= 240)), // Recompensa completa
            Times.Once);
        
        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(playerId, levelId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSkippingLevels_ShouldGrantFullReward()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 10;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 5); // Saltó niveles

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(playerId, levelId))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => 
                coins >= 160 && coins <= 240)), // Recompensa completa
            Times.Once);
        
        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(playerId, levelId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAlwaysGrantAtLeastOneCoin()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 1;
        const int worldId = 1; // Mundo más bajo con recompensa más baja
        
        var player = CreateTestPlayer(playerId, lastLevelId: 10);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        _playerRepositoryMock.Verify(
            x => x.AddCoinsAsync(playerId, It.Is<int>(coins => coins >= 1)),
            Times.Once);
    }

    #endregion

    #region Randomness Tests

    [Fact]
    public async Task ExecuteAsync_WhenCalledMultipleTimes_ShouldVaryRewards()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 10;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 5);
        var capturedCoins = new List<int>();

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Callback<int, int>((_, coins) => capturedCoins.Add(coins))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(playerId, levelId))
            .Returns(Task.CompletedTask);

        // Act - Ejecutar varias veces
        for (int i = 0; i < 10; i++)
        {
            await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);
        }

        // Assert
        capturedCoins.Should().HaveCount(10);
        capturedCoins.Should().OnlyContain(coins => coins >= 160 && coins <= 240);
        
        // Verificar que hay variación (al menos 2 valores diferentes)
        capturedCoins.Distinct().Should().HaveCountGreaterOrEqualTo(2);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 10;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 5);
        var callOrder = new List<string>();

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .Callback(() => callOrder.Add("GetByIdAsync"))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .Callback(() => callOrder.Add("AddCoinsAsync"))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(playerId, levelId))
            .Callback(() => callOrder.Add("UpdateLastLevelAsync"))
            .Returns(Task.CompletedTask);

        // Act
        await _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId);

        // Assert
        callOrder.Should().ContainInOrder("GetByIdAsync", "AddCoinsAsync", "UpdateLastLevelAsync");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAddCoinsFails_ShouldNotUpdateLastLevel()
    {
        // Arrange
        const int playerId = 100;
        const int levelId = 10;
        const int worldId = 2;
        
        var player = CreateTestPlayer(playerId, lastLevelId: 5);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(playerId, It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _grantLevelRewardUseCase.ExecuteAsync(playerId, levelId, worldId));

        _playerRepositoryMock.Verify(x => x.UpdateLastLevelAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Crea un jugador de prueba
    /// </summary>
    private static PlayerProfile CreateTestPlayer(int playerId, int lastLevelId)
    {
        return new PlayerProfile
        {
            Id = playerId,
            Uid = "test-uid-123",
            Name = "TestPlayer",
            Email = "test@test.com",
            LastLevelId = lastLevelId,
            Coins = 100,
            Points = 50
        };
    }

    #endregion
}