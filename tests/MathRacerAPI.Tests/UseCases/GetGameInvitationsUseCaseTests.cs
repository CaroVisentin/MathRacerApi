using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de obtención de invitaciones de partida (buzón)
/// </summary>
public class GetGameInvitationsUseCaseTests
{
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly Mock<IGameInvitationRepository> _mockInvitationRepository;
    private readonly GetGameInvitationsUseCase _getGameInvitationsUseCase;

    public GetGameInvitationsUseCaseTests()
    {
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockInvitationRepository = new Mock<IGameInvitationRepository>();

        _getGameInvitationsUseCase = new GetGameInvitationsUseCase(
            _mockPlayerRepository.Object,
            _mockInvitationRepository.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUid_ShouldReturnPendingInvitations()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Name = "Test Player",
            Uid = playerUid
        };

        var invitations = new List<GameInvitation>
        {
            new GameInvitation
            {
                Id = 1,
                GameId = 1001,
                InviterPlayerId = 2,
                InviterPlayerName = "Inviter1",
                InvitedPlayerId = playerId,
                InvitedPlayerName = "Test Player",
                Status = InvitationStatus.Pending,
                GameName = "Inviter1 vs Test Player",
                Difficulty = "facil",
                ExpectedResult = "MAYOR",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new GameInvitation
            {
                Id = 2,
                GameId = 1002,
                InviterPlayerId = 3,
                InviterPlayerName = "Inviter2",
                InvitedPlayerId = playerId,
                InvitedPlayerName = "Test Player",
                Status = InvitationStatus.Pending,
                GameName = "Inviter2 vs Test Player",
                Difficulty = "medio",
                ExpectedResult = "MENOR",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            }
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetPendingInvitationsForPlayerAsync(playerId))
            .ReturnsAsync(invitations);

        // Act
        var result = await _getGameInvitationsUseCase.ExecuteAsync(playerUid);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Should().OnlyContain(inv => inv.Status == InvitationStatus.Pending);
        result.Should().OnlyContain(inv => inv.InvitedPlayerId == playerId);

        _mockPlayerRepository.Verify(r => r.GetByUidAsync(playerUid), Times.Once);
        _mockInvitationRepository.Verify(r => r.GetPendingInvitationsForPlayerAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoInvitations_ShouldReturnEmptyList()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;

        var playerProfile = new PlayerProfile
        {
            Id = playerId,
            Name = "Test Player",
            Uid = playerUid
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetPendingInvitationsForPlayerAsync(playerId))
            .ReturnsAsync(new List<GameInvitation>());

        // Act
        var result = await _getGameInvitationsUseCase.ExecuteAsync(playerUid);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockInvitationRepository.Verify(r => r.GetPendingInvitationsForPlayerAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidUid_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidUid = "invalid-uid";

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(invalidUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _getGameInvitationsUseCase.ExecuteAsync(invalidUid)
        );

        _mockPlayerRepository.Verify(r => r.GetByUidAsync(invalidUid), Times.Once);
        _mockInvitationRepository.Verify(r => r.GetPendingInvitationsForPlayerAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOnlyReturnPendingInvitations()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };

        var allInvitations = new List<GameInvitation>
        {
            new GameInvitation
            {
                Id = 1,
                InvitedPlayerId = playerId,
                Status = InvitationStatus.Pending,
                GameName = "Game1",
                CreatedAt = DateTime.UtcNow
            },
            new GameInvitation
            {
                Id = 2,
                InvitedPlayerId = playerId,
                Status = InvitationStatus.Pending,
                GameName = "Game2",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetPendingInvitationsForPlayerAsync(playerId))
            .ReturnsAsync(allInvitations.Where(i => i.Status == InvitationStatus.Pending).ToList());

        // Act
        var result = await _getGameInvitationsUseCase.ExecuteAsync(playerUid);

        // Assert
        result.Should().OnlyContain(inv => inv.Status == InvitationStatus.Pending);
        result.Should().NotContain(inv => inv.Status == InvitationStatus.Accepted);
        result.Should().NotContain(inv => inv.Status == InvitationStatus.Rejected);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnInvitationsWithCompleteInformation()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };

        var invitation = new GameInvitation
        {
            Id = 1,
            GameId = 1001,
            InviterPlayerId = 2,
            InviterPlayerName = "Inviter",
            InvitedPlayerId = playerId,
            InvitedPlayerName = "Player",
            Status = InvitationStatus.Pending,
            GameName = "Inviter vs Player",
            Difficulty = "facil",
            ExpectedResult = "MAYOR",
            CreatedAt = DateTime.UtcNow
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetPendingInvitationsForPlayerAsync(playerId))
            .ReturnsAsync(new List<GameInvitation> { invitation });

        // Act
        var result = await _getGameInvitationsUseCase.ExecuteAsync(playerUid);

        // Assert
        var inv = result.First();
        inv.Id.Should().Be(1);
        inv.GameId.Should().Be(1001);
        inv.InviterPlayerName.Should().Be("Inviter");
        inv.GameName.Should().Be("Inviter vs Player");
        inv.Difficulty.Should().Be("facil");
        inv.ExpectedResult.Should().Be("MAYOR");
        inv.Status.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleInvitations_ShouldReturnAllPending()
    {
        // Arrange
        var playerUid = "player-uid-123";
        var playerId = 1;
        var playerProfile = new PlayerProfile { Id = playerId, Name = "Player", Uid = playerUid };

        var invitations = Enumerable.Range(1, 5).Select(i => new GameInvitation
        {
            Id = i,
            GameId = 1000 + i,
            InviterPlayerId = i + 10,
            InviterPlayerName = $"Inviter{i}",
            InvitedPlayerId = playerId,
            Status = InvitationStatus.Pending,
            GameName = $"Game {i}",
            Difficulty = "facil",
            ExpectedResult = "MAYOR",
            CreatedAt = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(playerUid)).ReturnsAsync(playerProfile);
        _mockInvitationRepository.Setup(r => r.GetPendingInvitationsForPlayerAsync(playerId))
            .ReturnsAsync(invitations);

        // Act
        var result = await _getGameInvitationsUseCase.ExecuteAsync(playerUid);

        // Assert
        result.Count.Should().Be(5);
        result.Should().OnlyContain(inv => inv.InvitedPlayerId == playerId);
        result.Should().OnlyContain(inv => inv.Status == InvitationStatus.Pending);
    }
}