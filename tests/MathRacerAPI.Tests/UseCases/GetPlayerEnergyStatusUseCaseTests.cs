using FluentAssertions;
using MathRacerAPI.Domain.Constants;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de obtención de estado de energía del jugador
/// </summary>
public class GetPlayerEnergyStatusUseCaseTests
{
    private readonly Mock<IEnergyRepository> _mockEnergyRepository;
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly GetPlayerEnergyStatusUseCase _useCase;

    public GetPlayerEnergyStatusUseCaseTests()
    {
        _mockEnergyRepository = new Mock<IEnergyRepository>();
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _useCase = new GetPlayerEnergyStatusUseCase(_mockEnergyRepository.Object, _mockPlayerRepository.Object);
    }

    #region ExecuteByUidAsync Tests

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerExists_ShouldReturnEnergyStatus()
    {
        // Arrange
        var uid = "test-uid-123";
        var playerId = 1;
        var player = new PlayerProfile { Id = playerId, Uid = uid };
        var energyData = (Amount: 2, LastConsumptionDate: DateTime.UtcNow.AddMinutes(-5));

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ReturnsAsync(player);
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.CurrentAmount.Should().Be(2);
        result.MaxAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        _mockPlayerRepository.Verify(r => r.GetByUidAsync(uid), Times.Once);
        _mockEnergyRepository.Verify(r => r.GetEnergyDataAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerNotFound_ShouldThrowArgumentException()
    {
        // Arrange
        var uid = "nonexistent-uid";
        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteByUidAsync(uid);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"No se encontró un jugador con UID: {uid}");
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_WhenEnergyDataIsNull_ShouldReturnMaxEnergy()
    {
        // Arrange
        var playerId = 1;
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync((ValueTuple<int, DateTime>?)null);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result.CurrentAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        result.MaxAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        result.SecondsUntilNextRecharge.Should().BeNull();
        _mockEnergyRepository.Verify(r => r.UpdateEnergyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEnergyAtMaximum_ShouldReturnMaxEnergyWithNoRechargeTime()
    {
        // Arrange
        var playerId = 1;
        var energyData = (Amount: 3, LastConsumptionDate: DateTime.UtcNow.AddHours(-1));
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        result.MaxAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        result.SecondsUntilNextRecharge.Should().BeNull();
        _mockEnergyRepository.Verify(r => r.UpdateEnergyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEnergyRecharges_ShouldUpdateDatabase()
    {
        // Arrange
        var playerId = 1;
        var lastConsumption = DateTime.UtcNow.AddMinutes(-30); // 30 minutos = 2 recargas de 15 min
        var energyData = (Amount: 1, LastConsumptionDate: lastConsumption);
        
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(3); // 1 + 2 recargas = 3 (máximo)
        result.MaxAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        result.SecondsUntilNextRecharge.Should().BeNull(); // Está al máximo
        
        // Verificar que se llamó UpdateEnergyAsync
        _mockEnergyRepository.Verify(r => r.UpdateEnergyAsync(
            playerId,
            3,
            It.Is<DateTime>(d => d > lastConsumption)
        ), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPartialRecharge_ShouldReturnCorrectSecondsUntilNext()
    {
        // Arrange
        var playerId = 1;
        var minutesPassed = 7; // 7 minutos transcurridos
        var lastConsumption = DateTime.UtcNow.AddMinutes(-minutesPassed);
        var energyData = (Amount: 2, LastConsumptionDate: lastConsumption);
        
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(2); // No alcanza para recargar (necesita 15 min)
        result.MaxAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        result.SecondsUntilNextRecharge.Should().NotBeNull();
        
        // Aproximadamente 8 minutos = 480 segundos restantes
        var expectedSeconds = (15 - minutesPassed) * 60;
        (result.SecondsUntilNextRecharge ?? 0).Should().BeInRange(expectedSeconds - 5, expectedSeconds + 5);

    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleRecharges_ShouldCalculateCorrectly()
    {
        // Arrange
        var playerId = 1;
        var minutesPassed = 35; // 35 minutos = 2 recargas completas (30 min) + 5 min progreso
        var lastConsumption = DateTime.UtcNow.AddMinutes(-minutesPassed);
        var energyData = (Amount: 1, LastConsumptionDate: lastConsumption);
        
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(3); // 1 + 2 recargas = 3 (máximo alcanzado)
        result.MaxAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        result.SecondsUntilNextRecharge.Should().BeNull(); // Llegó al máximo
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoRechargeYet_ShouldNotUpdateDatabase()
    {
        // Arrange
        var playerId = 1;
        var lastConsumption = DateTime.UtcNow.AddMinutes(-5); // Solo 5 minutos
        var energyData = (Amount: 2, LastConsumptionDate: lastConsumption);
        
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(2); // No cambió
        
        // No debería llamar a UpdateEnergyAsync
        _mockEnergyRepository.Verify(r => r.UpdateEnergyAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<DateTime>()
        ), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExactly15MinutesPassed_ShouldRechargeOneUnit()
    {
        // Arrange
        var playerId = 1;
        var lastConsumption = DateTime.UtcNow.AddMinutes(-15); // Exactamente 15 minutos
        var energyData = (Amount: 1, LastConsumptionDate: lastConsumption);
        
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(2); // 1 + 1 recarga
        result.SecondsUntilNextRecharge.Should().NotBeNull();
        // ~15 min = 900 seg
        (result.SecondsUntilNextRecharge ?? 0).Should().BeInRange(895, 905);
    }

    [Fact]
    public async Task ExecuteAsync_WhenZeroEnergy_ShouldRechargeCorrectly()
    {
        // Arrange
        var playerId = 1;
        var lastConsumption = DateTime.UtcNow.AddMinutes(-20); // 20 minutos = 1 recarga
        var energyData = (Amount: 0, LastConsumptionDate: lastConsumption);
        
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(1); // 0 + 1 recarga
        result.SecondsUntilNextRecharge.Should().NotBeNull();
        
        // ~10 minutos restantes = 600 segundos
        var expectedSeconds = (15 - 5) * 60; // 20 min pasados - 15 min de recarga = 5 min progreso
        (result.SecondsUntilNextRecharge ?? 0).Should().BeInRange(expectedSeconds - 10, expectedSeconds + 10);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(0, 50, 3)] // 50 minutos = 3 recargas, pero máximo es 3
    [InlineData(1, 45, 3)] // 45 minutos = 3 recargas, 1 + 3 = 4 pero máximo 3
    [InlineData(2, 30, 3)] // 30 minutos = 2 recargas, 2 + 2 = 4 pero máximo 3
    public async Task ExecuteAsync_WhenRechargeExceedsMaximum_ShouldCapAtMaxEnergy(
        int initialAmount, int minutesPassed, int expectedAmount)
    {
        // Arrange
        var playerId = 1;
        var lastConsumption = DateTime.UtcNow.AddMinutes(-minutesPassed);
        var energyData = (Amount: initialAmount, LastConsumptionDate: lastConsumption);
        
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(expectedAmount);
        result.CurrentAmount.Should().BeLessOrEqualTo(EnergyConstants.MAX_ENERGY);
    }

    [Fact]
    public async Task ExecuteAsync_WithVeryOldLastConsumption_ShouldRechargeToMaximum()
    {
        // Arrange
        var playerId = 1;
        var lastConsumption = DateTime.UtcNow.AddDays(-1); // 1 día atrás
        var energyData = (Amount: 0, LastConsumptionDate: lastConsumption);
        
        _mockEnergyRepository.Setup(r => r.GetEnergyDataAsync(playerId))
            .ReturnsAsync(energyData);

        // Act
        var result = await _useCase.ExecuteAsync(playerId);

        // Assert
        result.CurrentAmount.Should().Be(EnergyConstants.MAX_ENERGY);
        result.SecondsUntilNextRecharge.Should().BeNull();
    }

    #endregion
}