using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de eliminación lógica de jugador
/// </summary>
public class DeletePlayerUseCaseTests
{
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly DeletePlayerUseCase _deletePlayerUseCase;

    public DeletePlayerUseCaseTests()
    {
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _deletePlayerUseCase = new DeletePlayerUseCase(_playerRepositoryMock.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteAsync_WithValidUid_ShouldDeletePlayer()
    {
        // Arrange
        const string uid = "valid-uid-123";
        var existingPlayer = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(existingPlayer);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingPlayer_ShouldCallRepositoryInCorrectOrder()
    {
        // Arrange
        const string uid = "player-uid-456";
        var player = CreateTestPlayer(uid);
        var callOrder = new List<string>();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .Callback(() => callOrder.Add("GetByUid"))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Callback(() => callOrder.Add("Delete"))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        callOrder.Should().Equal("GetByUid", "Delete");
    }

    [Theory]
    [InlineData("uid-123")]
    [InlineData("firebase-uid-abc")]
    [InlineData("test-user-xyz")]
    public async Task ExecuteAsync_WithVariousValidUids_ShouldDeleteSuccessfully(string uid)
    {
        // Arrange
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPlayerWithProgress_ShouldStillDelete()
    {
        // Arrange
        const string uid = "advanced-player-uid";
        var advancedPlayer = new PlayerProfile
        {
            Id = 100,
            Uid = uid,
            Name = "Advanced Player",
            Email = "advanced@test.com",
            LastLevelId = 50,
            Points = 10000,
            Coins = 5000
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(advancedPlayer);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid), Times.Once);
    }

    #endregion

    #region Validation Tests - Invalid UID

    [Fact]
    public async Task ExecuteAsync_WithNullUid_ShouldThrowValidationException()
    {
        // Arrange
        string? uid = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _deletePlayerUseCase.ExecuteAsync(uid!));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
        _playerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyUid_ShouldThrowValidationException()
    {
        // Arrange
        const string uid = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _deletePlayerUseCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
        _playerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public async Task ExecuteAsync_WithWhitespaceUid_ShouldThrowValidationException(string uid)
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _deletePlayerUseCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("El UID es requerido");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(It.IsAny<string>()), Times.Never);
        _playerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Validation Tests - Player Not Found

    [Fact]
    public async Task ExecuteAsync_WhenPlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const string uid = "non-existent-uid";

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _deletePlayerUseCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("No se encontró un jugador con el UID proporcionado.");

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _playerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("deleted-player-uid")]
    [InlineData("unregistered-uid")]
    [InlineData("invalid-firebase-uid")]
    public async Task ExecuteAsync_WhenPlayerDoesNotExist_ShouldThrowNotFoundException(string uid)
    {
        // Arrange
        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _deletePlayerUseCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("No se encontró un jugador con el UID proporcionado.");

        _playerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerNotFound_ShouldNotAttemptDeletion()
    {
        // Arrange
        const string uid = "missing-player-uid";

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _deletePlayerUseCase.ExecuteAsync(uid));

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _playerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryMethodsOnce()
    {
        // Arrange
        const string uid = "test-uid";
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid), Times.Once);
        _playerRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallDeleteWithExactUid()
    {
        // Arrange
        const string uid = "exact-uid-to-verify";
        var player = CreateTestPlayer(uid);
        string? capturedUidForGet = null;
        string? capturedUidForDelete = null;

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(It.IsAny<string>()))
            .Callback<string>(u => capturedUidForGet = u)
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .Callback<string>(u => capturedUidForDelete = u)
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        capturedUidForGet.Should().Be(uid);
        capturedUidForDelete.Should().Be(uid);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotModifyPlayerBeforeDeletion()
    {
        // Arrange
        const string uid = "test-uid";
        var originalPlayer = CreateTestPlayer(uid, "Original Name", "original@test.com", 100, 5, 250);
        var originalName = originalPlayer.Name;
        var originalEmail = originalPlayer.Email;
        var originalPoints = originalPlayer.Points;

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(originalPlayer);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        originalPlayer.Name.Should().Be(originalName);
        originalPlayer.Email.Should().Be(originalEmail);
        originalPlayer.Points.Should().Be(originalPoints);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ExecuteAsync_WithNewPlayer_ShouldDeleteSuccessfully()
    {
        // Arrange
        const string uid = "new-player-uid";
        var newPlayer = new PlayerProfile
        {
            Id = 1,
            Uid = uid,
            Name = "New Player",
            Email = "new@test.com",
            LastLevelId = 0,
            Points = 0,
            Coins = 0
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(newPlayer);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleConsecutiveCalls_ShouldAttemptDeleteMultipleTimes()
    {
        // Arrange
        const string uid = "player-uid";
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid);
        await _deletePlayerUseCase.ExecuteAsync(uid);
        await _deletePlayerUseCase.ExecuteAsync(uid);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Exactly(3));
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid), Times.Exactly(3));
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentPlayers_ShouldDeleteCorrectPlayer()
    {
        // Arrange
        const string uid1 = "player1-uid";
        const string uid2 = "player2-uid";

        var player1 = CreateTestPlayer(uid1, "Player 1", "player1@test.com");
        var player2 = CreateTestPlayer(uid2, "Player 2", "player2@test.com");

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid1))
            .ReturnsAsync(player1);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid2))
            .ReturnsAsync(player2);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _deletePlayerUseCase.ExecuteAsync(uid1);
        await _deletePlayerUseCase.ExecuteAsync(uid2);

        // Assert
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid1), Times.Once);
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid2), Times.Once);
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task ExecuteAsync_WithConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        const string uid = "concurrent-uid";
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .Returns(Task.CompletedTask);

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _deletePlayerUseCase.ExecuteAsync(uid))
            .ToList();

        await Task.WhenAll(tasks);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Exactly(10));
        _playerRepositoryMock.Verify(x => x.DeleteAsync(uid), Times.Exactly(10));
    }

    [Fact]
    public async Task ExecuteAsync_WithConcurrentDifferentPlayers_ShouldDeleteAll()
    {
        // Arrange
        var playerUids = Enumerable.Range(1, 5).Select(i => $"player-{i}-uid").ToList();
        
        foreach (var uid in playerUids)
        {
            var player = CreateTestPlayer(uid);
            _playerRepositoryMock
                .Setup(x => x.GetByUidAsync(uid))
                .ReturnsAsync(player);

            _playerRepositoryMock
                .Setup(x => x.DeleteAsync(uid))
                .Returns(Task.CompletedTask);
        }

        // Act
        var tasks = playerUids.Select(uid => _deletePlayerUseCase.ExecuteAsync(uid)).ToList();
        await Task.WhenAll(tasks);

        // Assert
        foreach (var uid in playerUids)
        {
            _playerRepositoryMock.Verify(x => x.DeleteAsync(uid), Times.Once);
        }
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ExecuteAsync_WhenGetByUidThrowsException_ShouldPropagateException()
    {
        // Arrange
        const string uid = "error-uid";

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _deletePlayerUseCase.ExecuteAsync(uid));

        _playerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeleteThrowsException_ShouldPropagateException()
    {
        // Arrange
        const string uid = "delete-error-uid";
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _playerRepositoryMock
            .Setup(x => x.DeleteAsync(uid))
            .ThrowsAsync(new Exception("Delete operation failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _deletePlayerUseCase.ExecuteAsync(uid));

        exception.Message.Should().Contain("Delete operation failed");
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
            Coins = 0
        };
    }

    #endregion
}