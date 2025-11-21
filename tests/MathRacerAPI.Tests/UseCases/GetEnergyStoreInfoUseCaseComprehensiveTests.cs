using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios comprensivos para el caso de uso de obtener información de la tienda de energía
/// </summary>
public class GetEnergyStoreInfoUseCaseComprehensiveTests
{
    private readonly Mock<IEnergyRepository> _energyRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly GetEnergyStoreInfoUseCase _getEnergyStoreInfoUseCase;

    public GetEnergyStoreInfoUseCaseComprehensiveTests()
    {
        _energyRepositoryMock = new Mock<IEnergyRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _getEnergyStoreInfoUseCase = new GetEnergyStoreInfoUseCase(
            _energyRepositoryMock.Object,
            _playerRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPlayerId_ShouldThrowNotFoundException()
    {
        // Arrange
        const int invalidPlayerId = 999;

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidPlayerId))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await _getEnergyStoreInfoUseCase
            .Invoking(x => x.ExecuteAsync(invalidPlayerId))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Jugador no encontrado");
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingEnergyConfiguration_ShouldThrowBusinessException()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.GetEnergyConfigurationAsync())
            .ReturnsAsync(((int Price, int MaxAmount)?)null);

        // Act & Assert
        await _getEnergyStoreInfoUseCase
            .Invoking(x => x.ExecuteAsync(playerId))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Configuración de energía no encontrada");
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingPlayerEnergyData_ShouldThrowBusinessException()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
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
        await _getEnergyStoreInfoUseCase
            .Invoking(x => x.ExecuteAsync(playerId))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("No se pudo obtener la información de energía del jugador");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldReturnCorrectEnergyStoreInfo()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 75, MaxAmount: 15);
        var currentEnergyData = (Amount: 8, LastConsumptionDate: DateTime.UtcNow);

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result.PricePerUnit.Should().Be(75);
        result.MaxAmount.Should().Be(15);
        result.CurrentAmount.Should().Be(8);
        result.MaxCanBuy.Should().Be(7); // 15 - 8 = 7
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroCurrentEnergy_ShouldReturnMaxCanBuyEqualToMaxAmount()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 100, MaxAmount: 20);
        var currentEnergyData = (Amount: 0, LastConsumptionDate: DateTime.UtcNow);

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        result.PricePerUnit.Should().Be(100);
        result.MaxAmount.Should().Be(20);
        result.CurrentAmount.Should().Be(0);
        result.MaxCanBuy.Should().Be(20); // 20 - 0 = 20
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxCurrentEnergy_ShouldReturnZeroMaxCanBuy()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 60, MaxAmount: 12);
        var currentEnergyData = (Amount: 12, LastConsumptionDate: DateTime.UtcNow); // At max

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        result.PricePerUnit.Should().Be(60);
        result.MaxAmount.Should().Be(12);
        result.CurrentAmount.Should().Be(12);
        result.MaxCanBuy.Should().Be(0); // 12 - 12 = 0
    }

    [Fact]
    public async Task ExecuteAsync_WithCurrentEnergyAboveMax_ShouldReturnZeroMaxCanBuy()
    {
        // Arrange - This could happen in edge cases or data inconsistencies
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 45, MaxAmount: 10);
        var currentEnergyData = (Amount: 15, LastConsumptionDate: DateTime.UtcNow); // Above max

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        result.PricePerUnit.Should().Be(45);
        result.MaxAmount.Should().Be(10);
        result.CurrentAmount.Should().Be(15);
        result.MaxCanBuy.Should().Be(0); // Math.Max(0, 10 - 15) = 0
    }

    [Theory]
    [InlineData(25, 10, 3, 7)]
    [InlineData(50, 15, 0, 15)]
    [InlineData(75, 20, 20, 0)]
    [InlineData(100, 8, 4, 4)]
    [InlineData(30, 25, 12, 13)]
    public async Task ExecuteAsync_WithDifferentConfigurations_ShouldCalculateCorrectMaxCanBuy(
        int pricePerUnit, int maxAmount, int currentAmount, int expectedMaxCanBuy)
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: pricePerUnit, MaxAmount: maxAmount);
        var currentEnergyData = (Amount: currentAmount, LastConsumptionDate: DateTime.UtcNow);

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        result.PricePerUnit.Should().Be(pricePerUnit);
        result.MaxAmount.Should().Be(maxAmount);
        result.CurrentAmount.Should().Be(currentAmount);
        result.MaxCanBuy.Should().Be(expectedMaxCanBuy);
    }

    [Fact]
    public async Task ExecuteAsync_WithLowPriceConfiguration_ShouldReturnCorrectValues()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 1, MaxAmount: 100); // Very cheap energy
        var currentEnergyData = (Amount: 25, LastConsumptionDate: DateTime.UtcNow);

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        result.PricePerUnit.Should().Be(1);
        result.MaxAmount.Should().Be(100);
        result.CurrentAmount.Should().Be(25);
        result.MaxCanBuy.Should().Be(75);
    }

    [Fact]
    public async Task ExecuteAsync_WithHighPriceConfiguration_ShouldReturnCorrectValues()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 1000, MaxAmount: 5); // Very expensive energy
        var currentEnergyData = (Amount: 2, LastConsumptionDate: DateTime.UtcNow);

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        result.PricePerUnit.Should().Be(1000);
        result.MaxAmount.Should().Be(5);
        result.CurrentAmount.Should().Be(2);
        result.MaxCanBuy.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryMethodsOnce()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 50, MaxAmount: 10);
        var currentEnergyData = (Amount: 5, LastConsumptionDate: DateTime.UtcNow);

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByIdAsync(playerId), Times.Once);
        _energyRepositoryMock.Verify(x => x.GetEnergyConfigurationAsync(), Times.Once);
        _energyRepositoryMock.Verify(x => x.GetEnergyDataAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithMinimalConfiguration_ShouldWork()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 1, MaxAmount: 1); // Minimal configuration
        var currentEnergyData = (Amount: 0, LastConsumptionDate: DateTime.UtcNow);

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert
        result.PricePerUnit.Should().Be(1);
        result.MaxAmount.Should().Be(1);
        result.CurrentAmount.Should().Be(0);
        result.MaxCanBuy.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithHistoricalData_ShouldIgnoreLastConsumptionDate()
    {
        // Arrange - LastConsumptionDate should not affect the result
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var energyConfig = (Price: 40, MaxAmount: 8);
        var oldDate = DateTime.UtcNow.AddDays(-30); // Very old date
        var currentEnergyData = (Amount: 3, LastConsumptionDate: oldDate);

        SetupMocks(player, energyConfig, currentEnergyData);

        // Act
        var result = await _getEnergyStoreInfoUseCase.ExecuteAsync(playerId);

        // Assert - Should work regardless of LastConsumptionDate
        result.PricePerUnit.Should().Be(40);
        result.MaxAmount.Should().Be(8);
        result.CurrentAmount.Should().Be(3);
        result.MaxCanBuy.Should().Be(5);
    }

    #region Helper Methods

    private static PlayerProfile CreateTestPlayer(int id)
    {
        return new PlayerProfile
        {
            Id = id,
            Name = $"Player{id}",
            Email = $"player{id}@test.com",
            Coins = 1000,
            Points = 750,
            LastLevelId = 5
        };
    }

    private void SetupMocks(PlayerProfile player, (int Price, int MaxAmount) energyConfig,
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

    #endregion
}