using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios comprensivos para el caso de uso de compra de wildcards
/// </summary>
public class PurchaseWildcardUseCaseComprehensiveTests
{
    private readonly Mock<IWildcardRepository> _wildcardRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly PurchaseWildcardUseCase _purchaseWildcardUseCase;

    public PurchaseWildcardUseCaseComprehensiveTests()
    {
        _wildcardRepositoryMock = new Mock<IWildcardRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _purchaseWildcardUseCase = new PurchaseWildcardUseCase(
            _wildcardRepositoryMock.Object,
            _playerRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPlayerId_ShouldThrowNotFoundException()
    {
        // Arrange
        const int invalidPlayerId = 999;
        const int wildcardId = 1;

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidPlayerId))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await _purchaseWildcardUseCase
            .Invoking(x => x.ExecuteAsync(invalidPlayerId, wildcardId))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Jugador no encontrado");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task ExecuteAsync_WithInvalidQuantity_ShouldThrowValidationException(int invalidQuantity)
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        var player = CreateTestPlayer(playerId, 1000);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        // Act & Assert
        await _purchaseWildcardUseCase
            .Invoking(x => x.ExecuteAsync(playerId, wildcardId, invalidQuantity))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("La cantidad debe ser mayor a cero");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWildcardId_ShouldThrowNotFoundException()
    {
        // Arrange
        const int playerId = 1;
        const int invalidWildcardId = 999;
        var player = CreateTestPlayer(playerId, 1000);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _wildcardRepositoryMock
            .Setup(x => x.GetWildcardByIdAsync(invalidWildcardId))
            .ReturnsAsync((Wildcard?)null);

        // Act & Assert
        await _purchaseWildcardUseCase
            .Invoking(x => x.ExecuteAsync(playerId, invalidWildcardId))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Wildcard no encontrado");
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceedsMaxQuantity_ShouldThrowConflictException()
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        const int quantity = 5;
        var player = CreateTestPlayer(playerId, 1000);
        var wildcard = CreateTestWildcard(wildcardId, "Test Wildcard", 10);
        var currentPlayerWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard { WildcardId = wildcardId, Quantity = 98 }
        };

        SetupMocks(player, wildcard, currentPlayerWildcards, true);

        // Act & Assert
        await _purchaseWildcardUseCase
            .Invoking(x => x.ExecuteAsync(playerId, wildcardId, quantity))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("Solo puedes comprar 1 unidades más de este wildcard. Ya tienes 98/99");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlreadyAtMaxQuantity_ShouldThrowConflictException()
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        var player = CreateTestPlayer(playerId, 1000);
        var wildcard = CreateTestWildcard(wildcardId, "Test Wildcard", 10);
        var currentPlayerWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard { WildcardId = wildcardId, Quantity = 99 }
        };

        SetupMocks(player, wildcard, currentPlayerWildcards, true);

        // Act & Assert
        await _purchaseWildcardUseCase
            .Invoking(x => x.ExecuteAsync(playerId, wildcardId))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("Ya tienes la cantidad máxima de este wildcard (99 unidades)");
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientFunds_ShouldThrowInsufficientFundsException()
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        const int quantity = 5;
        var player = CreateTestPlayer(playerId, 30); // Solo 30 monedas
        var wildcard = CreateTestWildcard(wildcardId, "Expensive Wildcard", 10); // 10 monedas cada uno
        var currentPlayerWildcards = new List<PlayerWildcard>();

        SetupMocks(player, wildcard, currentPlayerWildcards, true);

        // Act & Assert
        await _purchaseWildcardUseCase
            .Invoking(x => x.ExecuteAsync(playerId, wildcardId, quantity))
            .Should().ThrowAsync<InsufficientFundsException>()
            .WithMessage("No tienes suficientes monedas. Necesitas 50, tienes 30");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ShouldThrowBusinessException()
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        const int quantity = 2;
        var player = CreateTestPlayer(playerId, 1000);
        var wildcard = CreateTestWildcard(wildcardId, "Test Wildcard", 10);
        var currentPlayerWildcards = new List<PlayerWildcard>();

        SetupMocks(player, wildcard, currentPlayerWildcards, false); // Purchase fails

        // Act & Assert
        await _purchaseWildcardUseCase
            .Invoking(x => x.ExecuteAsync(playerId, wildcardId, quantity))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Error al procesar la compra del wildcard");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidPurchase_ShouldReturnSuccessResult()
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        const int quantity = 3;
        var player = CreateTestPlayer(playerId, 1000);
        var wildcard = CreateTestWildcard(wildcardId, "Double Points", 15);
        var currentPlayerWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard { WildcardId = wildcardId, Quantity = 5 }
        };

        SetupMocks(player, wildcard, currentPlayerWildcards, true);

        // Act
        var result = await _purchaseWildcardUseCase.ExecuteAsync(playerId, wildcardId, quantity);

        // Assert
        result.success.Should().BeTrue();
        result.message.Should().Be("¡Compra exitosa! Compraste 3 unidades. Ahora tienes 8 unidades de Double Points");
        result.newQuantity.Should().Be(8);
        result.remainingCoins.Should().Be(955); // 1000 - 45

        _wildcardRepositoryMock.Verify(x => x.PurchaseWildcardAsync(playerId, wildcardId, quantity, 45), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithSingleQuantity_ShouldReturnCorrectMessage()
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        const int quantity = 1;
        var player = CreateTestPlayer(playerId, 1000);
        var wildcard = CreateTestWildcard(wildcardId, "Skip Question", 20);
        var currentPlayerWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard { WildcardId = wildcardId, Quantity = 2 }
        };

        SetupMocks(player, wildcard, currentPlayerWildcards, true);

        // Act
        var result = await _purchaseWildcardUseCase.ExecuteAsync(playerId, wildcardId, quantity);

        // Assert
        result.success.Should().BeTrue();
        result.message.Should().Be("¡Compra exitosa! Ahora tienes 3 unidades de Skip Question");
        result.newQuantity.Should().Be(3);
        result.remainingCoins.Should().Be(980); // 1000 - 20

        _wildcardRepositoryMock.Verify(x => x.PurchaseWildcardAsync(playerId, wildcardId, 1, 20), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNewWildcard_ShouldHandleZeroCurrentQuantity()
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        const int quantity = 2;
        var player = CreateTestPlayer(playerId, 1000);
        var wildcard = CreateTestWildcard(wildcardId, "New Wildcard", 25);
        var currentPlayerWildcards = new List<PlayerWildcard>(); // No wildcards yet

        SetupMocks(player, wildcard, currentPlayerWildcards, true);

        // Act
        var result = await _purchaseWildcardUseCase.ExecuteAsync(playerId, wildcardId, quantity);

        // Assert
        result.success.Should().BeTrue();
        result.message.Should().Be("¡Compra exitosa! Compraste 2 unidades. Ahora tienes 2 unidades de New Wildcard");
        result.newQuantity.Should().Be(2);
        result.remainingCoins.Should().Be(950); // 1000 - 50

        _wildcardRepositoryMock.Verify(x => x.PurchaseWildcardAsync(playerId, wildcardId, 2, 50), Times.Once);
    }

    [Theory]
    [InlineData(1, 10, 10)]
    [InlineData(5, 15, 75)]
    [InlineData(10, 20, 200)]
    [InlineData(25, 8, 200)]
    public async Task ExecuteAsync_WithDifferentQuantitiesAndPrices_ShouldCalculateCorrectTotalPrice(
        int quantity, int pricePerUnit, int expectedTotalPrice)
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        var player = CreateTestPlayer(playerId, 2000);
        var wildcard = CreateTestWildcard(wildcardId, "Test Wildcard", pricePerUnit);
        var currentPlayerWildcards = new List<PlayerWildcard>();

        SetupMocks(player, wildcard, currentPlayerWildcards, true);

        // Act
        var result = await _purchaseWildcardUseCase.ExecuteAsync(playerId, wildcardId, quantity);

        // Assert
        result.success.Should().BeTrue();
        result.newQuantity.Should().Be(quantity);
        result.remainingCoins.Should().Be(2000 - expectedTotalPrice);

        _wildcardRepositoryMock.Verify(x => x.PurchaseWildcardAsync(playerId, wildcardId, quantity, expectedTotalPrice), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaximumValidPurchase_ShouldSucceed()
    {
        // Arrange
        const int playerId = 1;
        const int wildcardId = 1;
        const int quantity = 99; // Maximum allowed
        var player = CreateTestPlayer(playerId, 10000);
        var wildcard = CreateTestWildcard(wildcardId, "Max Wildcard", 10);
        var currentPlayerWildcards = new List<PlayerWildcard>(); // Starting from zero

        SetupMocks(player, wildcard, currentPlayerWildcards, true);

        // Act
        var result = await _purchaseWildcardUseCase.ExecuteAsync(playerId, wildcardId, quantity);

        // Assert
        result.success.Should().BeTrue();
        result.newQuantity.Should().Be(99);
        result.message.Should().Be("¡Compra exitosa! Compraste 99 unidades. Ahora tienes 99 unidades de Max Wildcard");
        result.remainingCoins.Should().Be(9010); // 10000 - 990

        _wildcardRepositoryMock.Verify(x => x.PurchaseWildcardAsync(playerId, wildcardId, 99, 990), Times.Once);
    }

    #region Helper Methods

    private static PlayerProfile CreateTestPlayer(int id, int coins)
    {
        return new PlayerProfile
        {
            Id = id,
            Name = $"Player{id}",
            Email = $"player{id}@test.com",
            Coins = coins,
            Points = 100,
            LastLevelId = 1
        };
    }

    private static Wildcard CreateTestWildcard(int id, string name, double price)
    {
        return new Wildcard
        {
            Id = id,
            Name = name,
            Description = $"Description for {name}",
            Price = price
        };
    }

    private void SetupMocks(PlayerProfile player, Wildcard wildcard, List<PlayerWildcard> playerWildcards, bool purchaseSuccess)
    {
        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(player.Id))
            .ReturnsAsync(player);

        _wildcardRepositoryMock
            .Setup(x => x.GetWildcardByIdAsync(wildcard.Id))
            .ReturnsAsync(wildcard);

        _wildcardRepositoryMock
            .Setup(x => x.GetPlayerWildcardsAsync(player.Id))
            .ReturnsAsync(playerWildcards);

        _wildcardRepositoryMock
            .Setup(x => x.PurchaseWildcardAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(purchaseSuccess);
    }

    #endregion
}