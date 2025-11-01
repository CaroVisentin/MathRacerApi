using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de obtención de jugador por ID
/// </summary>
public class GetPlayerByIdUseCaseTests
{
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;

    public GetPlayerByIdUseCaseTests()
    {
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _getPlayerByIdUseCase = new GetPlayerByIdUseCase(_playerRepositoryMock.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteByUidAsync_WithValidUid_ShouldReturnPlayer()
    {
        // Arrange
        const string uid = "valid-uid-123";
        var expectedPlayer = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(expectedPlayer);

        // Act
        var result = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Uid.Should().Be(uid);
        result.Id.Should().Be(expectedPlayer.Id);
        result.Name.Should().Be(expectedPlayer.Name);
        result.Email.Should().Be(expectedPlayer.Email);
        result.LastLevelId.Should().Be(expectedPlayer.LastLevelId);
        result.Points.Should().Be(expectedPlayer.Points);
        result.Coins.Should().Be(expectedPlayer.Coins);

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WithDifferentPlayers_ShouldReturnCorrectData()
    {
        // Arrange
        const string uid1 = "player1-uid";
        const string uid2 = "player2-uid";
        
        var player1 = CreateTestPlayer(uid1, "Player 1", "player1@test.com", 100, 10, 500);
        var player2 = CreateTestPlayer(uid2, "Player 2", "player2@test.com", 200, 20, 1000);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid1))
            .ReturnsAsync(player1);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid2))
            .ReturnsAsync(player2);

        // Act
        var result1 = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid1);
        var result2 = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid2);

        // Assert
        result1.Uid.Should().Be(uid1);
        result1.Name.Should().Be("Player 1");
        result1.LastLevelId.Should().Be(10);

        result2.Uid.Should().Be(uid2);
        result2.Name.Should().Be("Player 2");
        result2.LastLevelId.Should().Be(20);
    }

    [Theory]
    [InlineData("uid-123")]
    [InlineData("firebase-uid-abc")]
    [InlineData("test-user-xyz")]
    public async Task ExecuteByUidAsync_WithVariousValidUids_ShouldReturnPlayer(string uid)
    {
        // Arrange
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act
        var result = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Uid.Should().Be(uid);
    }

    #endregion

    #region Validation Tests - Invalid UID

    [Fact]
    public async Task ExecuteByUidAsync_WithNullUid_ShouldThrowValidationException()
    {
        // Arrange
        string? uid = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _getPlayerByIdUseCase.ExecuteByUidAsync(uid!));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WithEmptyUid_ShouldThrowValidationException()
    {
        // Arrange
        const string uid = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _getPlayerByIdUseCase.ExecuteByUidAsync(uid));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public async Task ExecuteByUidAsync_WithWhitespaceUid_ShouldThrowValidationException(string uid)
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _getPlayerByIdUseCase.ExecuteByUidAsync(uid));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
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
            _getPlayerByIdUseCase.ExecuteByUidAsync(uid));

        exception.Message.Should().Contain("No se encontró un jugador con el UID proporcionado.");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
    }

    [Theory]
    [InlineData("deleted-player-uid")]
    [InlineData("unregistered-uid")]
    [InlineData("invalid-firebase-uid")]
    public async Task ExecuteByUidAsync_WhenPlayerDoesNotExist_ShouldThrowNotFoundException(string uid)
    {
        // Arrange
        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _getPlayerByIdUseCase.ExecuteByUidAsync(uid));

        exception.Message.Should().Contain("No se encontró un jugador con el UID proporcionado.");
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteByUidAsync_ShouldCallRepositoryOnce()
    {
        // Arrange
        const string uid = "test-uid";
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act
        await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _playerRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldCallRepositoryWithExactUid()
    {
        // Arrange
        const string uid = "exact-uid-to-verify";
        var player = CreateTestPlayer(uid);
        string? capturedUid = null;

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(It.IsAny<string>()))
            .Callback<string>(u => capturedUid = u)
            .ReturnsAsync(player);

        // Act
        await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        capturedUid.Should().Be(uid);
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldNotModifyRepositoryData()
    {
        // Arrange
        const string uid = "test-uid";
        var originalPlayer = CreateTestPlayer(uid, "Original Name", "original@test.com", 100, 5, 250);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(originalPlayer);

        // Act
        var result = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().BeSameAs(originalPlayer);
        result.Name.Should().Be("Original Name");
        result.Email.Should().Be("original@test.com");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ExecuteByUidAsync_WithNewPlayer_ShouldReturnPlayerWithDefaultValues()
    {
        // Arrange
        const string uid = "new-player-uid";
        var newPlayer = new PlayerProfile
        {
            Id = 1,
            Uid = uid,
            Name = "New Player",
            Email = "new@test.com",
            LastLevelId = 1,  // Nivel inicial
            Points = 0,       // Sin puntos
            Coins = 0         // Sin monedas
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(newPlayer);

        // Act
        var result = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.LastLevelId.Should().Be(1);
        result.Points.Should().Be(0);
        result.Coins.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WithAdvancedPlayer_ShouldReturnPlayerWithProgress()
    {
        // Arrange
        const string uid = "advanced-player-uid";
        var advancedPlayer = new PlayerProfile
        {
            Id = 100,
            Uid = uid,
            Name = "Pro Player",
            Email = "pro@test.com",
            LastLevelId = 50,   // Nivel avanzado
            Points = 10000,     // Muchos puntos
            Coins = 5000        // Muchas monedas
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(advancedPlayer);

        // Act
        var result = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.LastLevelId.Should().Be(50);
        result.Points.Should().Be(10000);
        result.Coins.Should().Be(5000);
    }

    [Fact]
    public async Task ExecuteByUidAsync_MultipleConsecutiveCalls_ShouldReturnConsistentData()
    {
        // Arrange
        const string uid = "consistent-uid";
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act
        var result1 = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);
        var result2 = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);
        var result3 = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        result1.Should().BeSameAs(player);
        result2.Should().BeSameAs(player);
        result3.Should().BeSameAs(player);

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Exactly(3));
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldReturnPlayerWithAllProperties()
    {
        // Arrange
        const string uid = "full-profile-uid";
        var player = CreateTestPlayer(uid, "Test Player", "test@example.com", 42, 15, 750);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act
        var result = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.Id.Should().Be(42);
        result.Uid.Should().Be(uid);
        result.Name.Should().Be("Test Player");
        result.Email.Should().Be("test@example.com");
        result.LastLevelId.Should().Be(15);
        result.Points.Should().Be(750);
        result.Coins.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(10, 100, 50)]
    [InlineData(25, 5000, 1000)]
    [InlineData(100, 99999, 50000)]
    public async Task ExecuteByUidAsync_WithVariousPlayerStats_ShouldReturnCorrectValues(
        int lastLevelId, 
        int points, 
        int coins)
    {
        // Arrange
        const string uid = "stats-test-uid";
        var player = CreateTestPlayer(uid, "Test Player", "test@test.com", 1, lastLevelId, points);
        player.Coins = coins;

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act
        var result = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        // Assert
        result.LastLevelId.Should().Be(lastLevelId);
        result.Points.Should().Be(points);
        result.Coins.Should().Be(coins);
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task ExecuteByUidAsync_WithConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        const string uid = "concurrent-uid";
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _getPlayerByIdUseCase.ExecuteByUidAsync(uid))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeEquivalentTo(player);
        results.Should().HaveCount(10);
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Exactly(10));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Crea un jugador de prueba con valores por defecto
    /// </summary>
    private static PlayerProfile CreateTestPlayer(
        string uid, 
        string name = "TestPlayer",
        string email = "test@test.com",
        int id = 1,
        int lastLevelId = 5,
        int points = 100)
    {
        return new PlayerProfile
        {
            Id = id,
            Uid = uid,
            Name = name,
            Email = email,
            LastLevelId = lastLevelId,
            Points = points,
            Coins = 50
        };
    }

    #endregion
}