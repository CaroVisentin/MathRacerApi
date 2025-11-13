using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios comprensivos para el caso de uso de compra de energía
/// </summary>
public class PurchaseEnergyUseCaseComprehensiveTests
{
    private readonly Mock<IEnergyRepository> _energyRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly PurchaseEnergyUseCase _purchaseEnergyUseCase;

    public PurchaseEnergyUseCaseComprehensiveTests()
    {
        _energyRepositoryMock = new Mock<IEnergyRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _purchaseEnergyUseCase = new PurchaseEnergyUseCase(
            _energyRepositoryMock.Object,
            _playerRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPlayerId_ShouldThrowNotFoundException()
    {
        // Arrange
        const int invalidPlayerId = 999;
        const int quantity = 1;

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidPlayerId))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(invalidPlayerId, quantity))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Jugador no encontrado");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task ExecuteAsync_WithInvalidQuantity_ShouldThrowValidationException(int invalidQuantity)
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId, 1000);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(playerId, invalidQuantity))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("La cantidad debe ser mayor a cero");
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingEnergyConfiguration_ShouldThrowBusinessException()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 1;
        var player = CreateTestPlayer(playerId, 1000);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.GetEnergyConfigurationAsync())
            .ReturnsAsync(((int Price, int MaxAmount)?)null);

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(playerId, quantity))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Configuración de energía no encontrada");
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingPlayerEnergyData_ShouldThrowBusinessException()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 1;
        var player = CreateTestPlayer(playerId, 1000);
        var energyConfig = (Price: 50, MaxAmount: 10);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.GetEnergyConfigurationAsync())
            .ReturnsAsync(energyConfig);

        _energyRepositoryMock
            .Setup(x => x.GetEnergyDataAsync(playerId))
            .ReturnsAsync(((int Amount, DateTime LastConsumptionDate)?)null);

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(playerId, quantity))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("No se pudo obtener la información de energía del jugador");
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceedsMaxEnergyLimit_ShouldThrowConflictException()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 5;
        var player = CreateTestPlayer(playerId, 1000);
        var energyConfig = (Price: 50, MaxAmount: 10);
        var currentEnergyData = (Amount: 8, LastConsumptionDate: DateTime.UtcNow);

        SetupBasicMocks(player, energyConfig, currentEnergyData);

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(playerId, quantity))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("Solo puedes comprar 2 unidades de energía. Ya tienes 8/10");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlreadyAtMaxEnergyLimit_ShouldThrowConflictException()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 1;
        var player = CreateTestPlayer(playerId, 1000);
        var energyConfig = (Price: 50, MaxAmount: 10);
        var currentEnergyData = (Amount: 10, LastConsumptionDate: DateTime.UtcNow); // Already at max

        SetupBasicMocks(player, energyConfig, currentEnergyData);

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(playerId, quantity))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("Ya tienes la cantidad máxima de energía permitida");
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientFunds_ShouldThrowInsufficientFundsException()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 3;
        var player = CreateTestPlayer(playerId, 100); // Only 100 coins
        var energyConfig = (Price: 50, MaxAmount: 10); // 50 coins per unit
        var currentEnergyData = (Amount: 5, LastConsumptionDate: DateTime.UtcNow);

        SetupBasicMocks(player, energyConfig, currentEnergyData);

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(playerId, quantity))
            .Should().ThrowAsync<InsufficientFundsException>()
            .WithMessage("No tienes suficientes monedas. Necesitas 150, tienes 100");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ShouldThrowBusinessException()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 2;
        var player = CreateTestPlayer(playerId, 1000);
        var energyConfig = (Price: 50, MaxAmount: 10);
        var currentEnergyData = (Amount: 3, LastConsumptionDate: DateTime.UtcNow);

        SetupBasicMocks(player, energyConfig, currentEnergyData);

        _energyRepositoryMock
            .Setup(x => x.PurchaseEnergyAsync(playerId, quantity, 100))
            .ReturnsAsync(false); // Purchase fails

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(playerId, quantity))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Error al procesar la compra de energía");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUpdatedPlayerNotFound_ShouldThrowBusinessException()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 2;
        var player = CreateTestPlayer(playerId, 1000);
        var energyConfig = (Price: 50, MaxAmount: 10);
        var currentEnergyData = (Amount: 3, LastConsumptionDate: DateTime.UtcNow);

        SetupBasicMocks(player, energyConfig, currentEnergyData);

        _energyRepositoryMock
            .Setup(x => x.PurchaseEnergyAsync(playerId, quantity, 100))
            .ReturnsAsync(true);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player) // First call succeeds
            .Callback(() =>
            {
                // Second call fails
                _playerRepositoryMock
                    .Setup(x => x.GetByIdAsync(playerId))
                    .ReturnsAsync((PlayerProfile?)null);
            });

        // Act & Assert
        await _purchaseEnergyUseCase
            .Invoking(x => x.ExecuteAsync(playerId, quantity))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("No se pudo obtener la información actualizada del jugador");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSinglePurchase_ShouldReturnCorrectResult()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 1;
        var player = CreateTestPlayer(playerId, 1000);
        var energyConfig = (Price: 75, MaxAmount: 15);
        var currentEnergyData = (Amount: 5, LastConsumptionDate: DateTime.UtcNow);
        var updatedPlayer = CreateTestPlayer(playerId, 925); // 1000 - 75 = 925

        SetupSuccessfulPurchase(player, energyConfig, currentEnergyData, updatedPlayer, quantity);

        // Act
        var result = await _purchaseEnergyUseCase.ExecuteAsync(playerId, quantity);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Compra exitosa: 1 unidad de energía");
        result.NewEnergyAmount.Should().Be(6); // 5 + 1
        result.RemainingCoins.Should().Be(925);
        result.TotalPrice.Should().Be(75);

        _energyRepositoryMock.Verify(x => x.PurchaseEnergyAsync(playerId, 1, 75), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidMultiplePurchase_ShouldReturnCorrectResult()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 4;
        var player = CreateTestPlayer(playerId, 2000);
        var energyConfig = (Price: 60, MaxAmount: 20);
        var currentEnergyData = (Amount: 8, LastConsumptionDate: DateTime.UtcNow);
        var updatedPlayer = CreateTestPlayer(playerId, 1760); // 2000 - 240 = 1760

        SetupSuccessfulPurchase(player, energyConfig, currentEnergyData, updatedPlayer, quantity);

        // Act
        var result = await _purchaseEnergyUseCase.ExecuteAsync(playerId, quantity);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Compra exitosa: 4 unidades de energía");
        result.NewEnergyAmount.Should().Be(12); // 8 + 4
        result.RemainingCoins.Should().Be(1760);
        result.TotalPrice.Should().Be(240); // 60 * 4

        _energyRepositoryMock.Verify(x => x.PurchaseEnergyAsync(playerId, 4, 240), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaximumValidPurchase_ShouldSucceed()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 10; // Buying to reach maximum
        var player = CreateTestPlayer(playerId, 5000);
        var energyConfig = (Price: 100, MaxAmount: 15);
        var currentEnergyData = (Amount: 5, LastConsumptionDate: DateTime.UtcNow);
        var updatedPlayer = CreateTestPlayer(playerId, 4000); // 5000 - 1000 = 4000

        SetupSuccessfulPurchase(player, energyConfig, currentEnergyData, updatedPlayer, quantity);

        // Act
        var result = await _purchaseEnergyUseCase.ExecuteAsync(playerId, quantity);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.NewEnergyAmount.Should().Be(15); // 5 + 10 = max
        result.RemainingCoins.Should().Be(4000);
        result.TotalPrice.Should().Be(1000); // 100 * 10
    }

    [Theory]
    [InlineData(1, 50, 50)]
    [InlineData(3, 25, 75)]
    [InlineData(5, 40, 200)]
    [InlineData(10, 15, 150)]
    public async Task ExecuteAsync_WithDifferentQuantitiesAndPrices_ShouldCalculateCorrectTotalPrice(
        int quantity, int pricePerUnit, int expectedTotalPrice)
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId, 5000);
        var energyConfig = (Price: pricePerUnit, MaxAmount: 20);
        var currentEnergyData = (Amount: 2, LastConsumptionDate: DateTime.UtcNow);
        var updatedPlayer = CreateTestPlayer(playerId, 5000 - expectedTotalPrice);

        SetupSuccessfulPurchase(player, energyConfig, currentEnergyData, updatedPlayer, quantity);

        // Act
        var result = await _purchaseEnergyUseCase.ExecuteAsync(playerId, quantity);

        // Assert
        result.Success.Should().BeTrue();
        result.TotalPrice.Should().Be(expectedTotalPrice);
        result.NewEnergyAmount.Should().Be(2 + quantity);

        _energyRepositoryMock.Verify(x => x.PurchaseEnergyAsync(playerId, quantity, expectedTotalPrice), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 2;
        var player = CreateTestPlayer(playerId, 1000);
        var energyConfig = (Price: 50, MaxAmount: 10);
        var currentEnergyData = (Amount: 3, LastConsumptionDate: DateTime.UtcNow);
        var updatedPlayer = CreateTestPlayer(playerId, 900);

        SetupSuccessfulPurchase(player, energyConfig, currentEnergyData, updatedPlayer, quantity);

        // Act
        await _purchaseEnergyUseCase.ExecuteAsync(playerId, quantity);

        // Assert - Verify all repository calls were made in correct sequence
        _playerRepositoryMock.Verify(x => x.GetByIdAsync(playerId), Times.Exactly(2));
        _energyRepositoryMock.Verify(x => x.GetEnergyConfigurationAsync(), Times.Once);
        _energyRepositoryMock.Verify(x => x.GetEnergyDataAsync(playerId), Times.Once);
        _energyRepositoryMock.Verify(x => x.PurchaseEnergyAsync(playerId, quantity, 100), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroCurrentEnergy_ShouldWorkCorrectly()
    {
        // Arrange
        const int playerId = 1;
        const int quantity = 3;
        var player = CreateTestPlayer(playerId, 500);
        var energyConfig = (Price: 30, MaxAmount: 10);
        var currentEnergyData = (Amount: 0, LastConsumptionDate: DateTime.UtcNow); // Starting from zero
        var updatedPlayer = CreateTestPlayer(playerId, 410); // 500 - 90 = 410

        SetupSuccessfulPurchase(player, energyConfig, currentEnergyData, updatedPlayer, quantity);

        // Act
        var result = await _purchaseEnergyUseCase.ExecuteAsync(playerId, quantity);

        // Assert
        result.Success.Should().BeTrue();
        result.NewEnergyAmount.Should().Be(3); // 0 + 3
        result.RemainingCoins.Should().Be(410);
        result.TotalPrice.Should().Be(90); // 30 * 3
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
            Points = 500,
            LastLevelId = 3
        };
    }

    private void SetupBasicMocks(PlayerProfile player, (int Price, int MaxAmount) energyConfig, 
        (int Amount, DateTime LastConsumptionDate) currentEnergyData)
    {
        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(player.Id))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.GetEnergyConfigurationAsync())
            .ReturnsAsync(energyConfig);

        _energyRepositoryMock
            .Setup(x => x.GetEnergyDataAsync(player.Id))
            .ReturnsAsync(currentEnergyData);
    }

    private void SetupSuccessfulPurchase(PlayerProfile player, (int Price, int MaxAmount) energyConfig,
        (int Amount, DateTime LastConsumptionDate) currentEnergyData, PlayerProfile updatedPlayer, int quantity)
    {
        SetupBasicMocks(player, energyConfig, currentEnergyData);

        var totalPrice = energyConfig.Price * quantity;

        _energyRepositoryMock
            .Setup(x => x.PurchaseEnergyAsync(player.Id, quantity, totalPrice))
            .ReturnsAsync(true);

        // Setup the sequence of calls to GetByIdAsync
        var sequence = new MockSequence();
        _playerRepositoryMock.InSequence(sequence)
            .Setup(x => x.GetByIdAsync(player.Id))
            .ReturnsAsync(player);

        _playerRepositoryMock.InSequence(sequence)
            .Setup(x => x.GetByIdAsync(player.Id))
            .ReturnsAsync(updatedPlayer);
    }

    #endregion
}