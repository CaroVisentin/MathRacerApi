using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de responder a invitaciones de partida
/// </summary>
public class RespondGameInvitationUseCaseTests
{
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly Mock<IGameInvitationRepository> _mockInvitationRepository;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly RespondGameInvitationUseCase _respondGameInvitationUseCase;

    public RespondGameInvitationUseCaseTests()
    {
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockInvitationRepository = new Mock<IGameInvitationRepository>();
        _mockGameRepository = new Mock<IGameRepository>();

        _respondGameInvitationUseCase = new RespondGameInvitationUseCase(
            _mockPlayerRepository.Object,
            _mockInvitationRepository.Object,
            _mockGameRepository.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_AcceptInvitation_ShouldReturnTrueAndGameId()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invitationId = 10;
        var gameId = 1001;

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Name = "Test Player",
            Uid = playerUid
        };

        var invitation = new GameInvitation
        {
            Id = invitationId,
            GameId = gameId,
            InviterPlayerId = 2,
            InvitedPlayerId = playerId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invitationId))
            .ReturnsAsync(invitation);
        _mockInvitationRepository.Setup(r => r.UpdateStatusAsync(invitationId, InvitationStatus.Accepted))
            .Returns(Task.CompletedTask);

        // Act
        var (accepted, returnedGameId) = await _respondGameInvitationUseCase.ExecuteAsync(
            playerUid,
            invitationId,
            accept: true
        );

        // Assert
        accepted.Should().BeTrue();
        returnedGameId.Should().Be(gameId);

        _mockInvitationRepository.Verify(r => r.UpdateStatusAsync(invitationId, InvitationStatus.Accepted), Times.Once);
        _mockPlayerRepository.Verify(r => r.GetByUidAsync(playerUid), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_RejectInvitation_ShouldReturnFalseAndNoGameId()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invitationId = 10;

        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };
        var invitation = new GameInvitation
        {
            Id = invitationId,
            GameId = 1001,
            InviterPlayerId = 2,
            InvitedPlayerId = playerId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);
        _mockInvitationRepository.Setup(r => r.UpdateStatusAsync(invitationId, InvitationStatus.Rejected))
            .Returns(Task.CompletedTask);
        _mockGameRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Game?)null);

        // Act
        var (accepted, returnedGameId) = await _respondGameInvitationUseCase.ExecuteAsync(
            playerUid,
            invitationId,
            accept: false
        );

        // Assert
        accepted.Should().BeFalse();
        returnedGameId.Should().BeNull();

        _mockInvitationRepository.Verify(r => r.UpdateStatusAsync(invitationId, InvitationStatus.Rejected), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPlayerUid_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidUid = "invalid-uid";

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(invalidUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _respondGameInvitationUseCase.ExecuteAsync(invalidUid, 1, true)
        );

        _mockInvitationRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockInvitationRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<InvitationStatus>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidInvitationId_ShouldThrowNotFoundException()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invalidInvitationId = 999;

        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invalidInvitationId))
            .ReturnsAsync((GameInvitation?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _respondGameInvitationUseCase.ExecuteAsync(playerUid, invalidInvitationId, true)
        );

        _mockInvitationRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<InvitationStatus>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerIsNotInvited_ShouldThrowValidationException()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invitationId = 10;

        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };
        var invitation = new GameInvitation
        {
            Id = invitationId,
            GameId = 1001,
            InviterPlayerId = 2,
            InvitedPlayerId = 99, // Diferente al jugador actual
            Status = InvitationStatus.Pending
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _respondGameInvitationUseCase.ExecuteAsync(playerUid, invitationId, true)
        );

        _mockInvitationRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<InvitationStatus>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvitationAlreadyAccepted_ShouldThrowValidationException()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invitationId = 10;

        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };
        var invitation = new GameInvitation
        {
            Id = invitationId,
            GameId = 1001,
            InvitedPlayerId = playerId,
            Status = InvitationStatus.Accepted, // Ya aceptada
            RespondedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _respondGameInvitationUseCase.ExecuteAsync(playerUid, invitationId, true)
        );

        _mockInvitationRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<InvitationStatus>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvitationAlreadyRejected_ShouldThrowValidationException()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invitationId = 10;

        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };
        var invitation = new GameInvitation
        {
            Id = invitationId,
            GameId = 1001,
            InvitedPlayerId = playerId,
            Status = InvitationStatus.Rejected, // Ya rechazada
            RespondedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _respondGameInvitationUseCase.ExecuteAsync(playerUid, invitationId, false)
        );

        _mockInvitationRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<InvitationStatus>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_ShouldUpdateInvitationStatusCorrectly(bool accept)
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invitationId = 10;

        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };
        var invitation = new GameInvitation
        {
            Id = invitationId,
            GameId = 1001,
            InvitedPlayerId = playerId,
            Status = InvitationStatus.Pending
        };

        var expectedStatus = accept ? InvitationStatus.Accepted : InvitationStatus.Rejected;

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);
        _mockInvitationRepository.Setup(r => r.UpdateStatusAsync(invitationId, expectedStatus))
            .Returns(Task.CompletedTask);
        _mockGameRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Game?)null);

        // Act
        await _respondGameInvitationUseCase.ExecuteAsync(playerUid, invitationId, accept);

        // Assert
        _mockInvitationRepository.Verify(r => r.UpdateStatusAsync(invitationId, expectedStatus), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptInvitation_ShouldReturnCorrectGameId()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invitationId = 10;
        var expectedGameId = 1001;

        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };
        var invitation = new GameInvitation
        {
            Id = invitationId,
            GameId = expectedGameId,
            InvitedPlayerId = playerId,
            Status = InvitationStatus.Pending
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);
        _mockInvitationRepository.Setup(r => r.UpdateStatusAsync(invitationId, InvitationStatus.Accepted))
            .Returns(Task.CompletedTask);

        // Act
        var (accepted, gameId) = await _respondGameInvitationUseCase.ExecuteAsync(
            playerUid,
            invitationId,
            accept: true
        );

        // Assert
        accepted.Should().BeTrue();
        gameId.Should().Be(expectedGameId);
    }

    [Fact]
    public async Task ExecuteAsync_RejectInvitation_ShouldNotReturnGameId()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var invitationId = 10;

        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };
        var invitation = new GameInvitation
        {
            Id = invitationId,
            GameId = 1001,
            InvitedPlayerId = playerId,
            Status = InvitationStatus.Pending
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetByIdAsync(invitationId)).ReturnsAsync(invitation);
        _mockInvitationRepository.Setup(r => r.UpdateStatusAsync(invitationId, InvitationStatus.Rejected))
            .Returns(Task.CompletedTask);
        _mockGameRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Game?)null);

        // Act
        var (accepted, gameId) = await _respondGameInvitationUseCase.ExecuteAsync(
            playerUid,
            invitationId,
            accept: false
        );

        // Assert
        accepted.Should().BeFalse();
        gameId.Should().BeNull();
    }
}