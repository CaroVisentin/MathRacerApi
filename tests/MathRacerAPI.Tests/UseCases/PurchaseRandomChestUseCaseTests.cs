using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de compra de cofre aleatorio
/// </summary>
public class PurchaseRandomChestUseCaseTests
{
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly PurchaseRandomChestUseCase _useCase;

    public PurchaseRandomChestUseCaseTests()
    {
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _useCase = new PurchaseRandomChestUseCase(
            _playerRepositoryMock.Object,
            _storeRepositoryMock.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteAsync_WithValidUidAndSufficientCoins_ShouldReturnTrue()
    {
        // Arrange
        const string uid = "valid-uid-123";
        const int playerId = 1;
        const int playerCoins = 5000;
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, chestPrice))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(uid);

        // Assert
        result.Should().BeTrue();
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(playerId, chestPrice), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExactCoinsRequired_ShouldReturnTrue()
    {
        // Arrange
        const string uid = "uid-exact-coins";
        const int playerId = 2;
        const int playerCoins = 3000; // Exactly 3000
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, chestPrice))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(uid);

        // Assert
        result.Should().BeTrue();
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(playerId, chestPrice), Times.Once);
    }

    [Theory]
    [InlineData("uid-1", 3000)]
    [InlineData("uid-2", 5000)]
    [InlineData("uid-3", 10000)]
    [InlineData("uid-4", 50000)]
    public async Task ExecuteAsync_WithVariousValidCoins_ShouldReturnTrue(string uid, int coins)
    {
        // Arrange
        const int playerId = 1;
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, coins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, chestPrice))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(uid);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Validation Tests - Invalid UID

    [Fact]
    public async Task ExecuteAsync_WithNullUid_ShouldThrowValidationException()
    {
        // Arrange
        string? uid = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(uid!));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyUid_ShouldThrowValidationException()
    {
        // Arrange
        const string uid = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public async Task ExecuteAsync_WithWhitespaceUid_ShouldThrowValidationException(string uid)
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Validation Tests - Player Not Found

    [Fact]
    public async Task ExecuteAsync_WithNonExistentPlayer_ShouldThrowNotFoundException()
    {
        // Arrange
        const string uid = "non-existent-uid";

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("Jugador no encontrado");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData("deleted-player-uid")]
    [InlineData("unregistered-uid")]
    [InlineData("invalid-firebase-uid")]
    public async Task ExecuteAsync_WithInvalidPlayer_ShouldThrowNotFoundException(string uid)
    {
        // Arrange
        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("Jugador no encontrado");

        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Validation Tests - Insufficient Funds

    [Fact]
    public async Task ExecuteAsync_WithInsufficientCoins_ShouldThrowInsufficientFundsException()
    {
        // Arrange
        const string uid = "uid-poor-player";
        const int playerId = 1;
        const int playerCoins = 2000; // Less than 3000 required
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InsufficientFundsException>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain($"Necesitas {chestPrice}");
        exception.Message.Should().Contain($"tienes {playerCoins}");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(2999)]
    public async Task ExecuteAsync_WithVariousInsufficientCoins_ShouldThrowInsufficientFundsException(int coins)
    {
        // Arrange
        const string uid = "uid-test";
        const int playerId = 1;
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, coins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InsufficientFundsException>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain($"Necesitas {chestPrice}");
        exception.Message.Should().Contain($"tienes {coins}");

        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroCoins_ShouldThrowInsufficientFundsException()
    {
        // Arrange
        const string uid = "uid-broke-player";
        const int playerId = 1;
        const int playerCoins = 0;

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InsufficientFundsException>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("No tienes suficientes monedas");

        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoriesInCorrectOrder()
    {
        // Arrange
        const string uid = "uid-test-order";
        const int playerId = 1;
        const int playerCoins = 5000;
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);
        var callOrder = new List<string>();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .Callback(() => callOrder.Add("GetByUid"))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, chestPrice))
            .Callback(() => callOrder.Add("PurchaseChest"))
            .ReturnsAsync(true);

        // Act
        await _useCase.ExecuteAsync(uid);

        // Assert
        callOrder.Should().Equal("GetByUid", "PurchaseChest");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoriesOnce()
    {
        // Arrange
        const string uid = "uid-test";
        const int playerId = 1;
        const int playerCoins = 5000;
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, chestPrice))
            .ReturnsAsync(true);

        // Act
        await _useCase.ExecuteAsync(uid);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(playerId, chestPrice), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallPurchaseWithCorrectPrice()
    {
        // Arrange
        const string uid = "uid-test-price";
        const int playerId = 1;
        const int playerCoins = 5000;
        const int expectedChestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);
        int? capturedPrice = null;

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, It.IsAny<int>()))
            .Callback<int, int>((_, price) => capturedPrice = price)
            .ReturnsAsync(true);

        // Act
        await _useCase.ExecuteAsync(uid);

        // Assert
        capturedPrice.Should().Be(expectedChestPrice);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallPurchaseWithCorrectPlayerId()
    {
        // Arrange
        const string uid = "uid-test-player-id";
        const int expectedPlayerId = 42;
        const int playerCoins = 5000;
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, expectedPlayerId, playerCoins);
        int? capturedPlayerId = null;

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), chestPrice))
            .Callback<int, int>((pId, _) => capturedPlayerId = pId)
            .ReturnsAsync(true);

        // Act
        await _useCase.ExecuteAsync(uid);

        // Assert
        capturedPlayerId.Should().Be(expectedPlayerId);
    }

    #endregion

    #region Purchase Failure Tests

    [Fact]
    public async Task ExecuteAsync_WhenPurchaseFails_ShouldReturnFalse()
    {
        // Arrange
        const string uid = "uid-purchase-fails";
        const int playerId = 1;
        const int playerCoins = 5000;
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, chestPrice))
            .ReturnsAsync(false); // Purchase fails

        // Act
        var result = await _useCase.ExecuteAsync(uid);

        // Assert
        result.Should().BeFalse();
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(playerId, chestPrice), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        const string uid = "uid-repository-error";
        const int playerId = 1;
        const int playerCoins = 5000;
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, chestPrice))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("Database error");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ExecuteAsync_WithMultipleConsecutiveCalls_ShouldProcessEach()
    {
        // Arrange
        const string uid = "uid-multiple-calls";
        const int playerId = 1;
        const int playerCoins = 10000; // Enough for multiple purchases
        const int chestPrice = 3000;

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(playerId, chestPrice))
            .ReturnsAsync(true);

        // Act
        var result1 = await _useCase.ExecuteAsync(uid);
        var result2 = await _useCase.ExecuteAsync(uid);
        var result3 = await _useCase.ExecuteAsync(uid);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Exactly(3));
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(playerId, chestPrice), Times.Exactly(3));
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentPlayers_ShouldProcessIndependently()
    {
        // Arrange
        const string uid1 = "uid-player-1";
        const string uid2 = "uid-player-2";
        const int playerId1 = 1;
        const int playerId2 = 2;
        const int chestPrice = 3000;

        var player1 = CreateTestPlayer(uid1, playerId1, 5000);
        var player2 = CreateTestPlayer(uid2, playerId2, 4000);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid1))
            .ReturnsAsync(player1);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid2))
            .ReturnsAsync(player2);

        _storeRepositoryMock
            .Setup(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), chestPrice))
            .ReturnsAsync(true);

        // Act
        var result1 = await _useCase.ExecuteAsync(uid1);
        var result2 = await _useCase.ExecuteAsync(uid2);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();

        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(playerId1, chestPrice), Times.Once);
        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(playerId2, chestPrice), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeCoins_ShouldThrowInsufficientFundsException()
    {
        // Arrange
        const string uid = "uid-negative-coins";
        const int playerId = 1;
        const int playerCoins = -100; // Edge case: negative coins

        var player = CreateTestPlayer(uid, playerId, playerCoins);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InsufficientFundsException>(
            () => _useCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("No tienes suficientes monedas");

        _storeRepositoryMock.Verify(x => x.PurchaseRandomChestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Crea un jugador de prueba con valores por defecto
    /// </summary>
    private static PlayerProfile CreateTestPlayer(string uid, int id, int coins)
    {
        return new PlayerProfile
        {
            Id = id,
            Uid = uid,
            Name = "Test Player",
            Email = "test@test.com",
            Coins = coins,
            Points = 100,
            LastLevelId = 1
        };
    }

    #endregion
}