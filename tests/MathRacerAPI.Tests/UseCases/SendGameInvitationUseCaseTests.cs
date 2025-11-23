using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de envío de invitaciones de partida
/// </summary>
public class SendGameInvitationUseCaseTests
{
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly Mock<IGameInvitationRepository> _mockInvitationRepository;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<ICreateCustomOnlineGameUseCase> _mockCreateGameUseCase; 
    private readonly SendGameInvitationUseCase _sendGameInvitationUseCase;

    public SendGameInvitationUseCaseTests()
    {
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockInvitationRepository = new Mock<IGameInvitationRepository>();
        _mockGameRepository = new Mock<IGameRepository>();
        _mockCreateGameUseCase = new Mock<ICreateCustomOnlineGameUseCase>(); 

        _sendGameInvitationUseCase = new SendGameInvitationUseCase(
            _mockPlayerRepository.Object,
            _mockInvitationRepository.Object,
            _mockGameRepository.Object,
            _mockCreateGameUseCase.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateInvitationSuccessfully()
    {
        // Arrange
        var inviterUid = "inviter-uid-123";
        var invitedFriendId = 2;
        var difficulty = "facil";
        var expectedResult = "MAYOR";

        var inviterProfile = new PlayerProfile
        {
            Id = 1,
            Name = "Inviter Player",
            Uid = inviterUid,
            Email = "inviter@test.com"
        };

        var invitedProfile = new PlayerProfile
        {
            Id = invitedFriendId,
            Name = "Invited Friend",
            Uid = "invited-uid-456",
            Email = "invited@test.com"
        };

        var createdGame = new Game
        {
            Id = 1001,
            Name = "Inviter Player vs Invited Friend",
            IsPrivate = false,
            Status = GameStatus.WaitingForPlayers,
            ExpectedResult = expectedResult,
            Questions = new List<Question>()
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(inviterUid))
            .ReturnsAsync(inviterProfile);
        _mockPlayerRepository.Setup(r => r.GetByIdAsync(invitedFriendId))
            .ReturnsAsync(invitedProfile);
        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(
            inviterUid,
            It.IsAny<string>(),
            false,
            null,
            difficulty,
            expectedResult
        )).ReturnsAsync(createdGame);
        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);
        _mockInvitationRepository.Setup(r => r.CreateAsync(It.IsAny<GameInvitation>()))
            .ReturnsAsync((GameInvitation inv) => inv);

        // Act
        var result = await _sendGameInvitationUseCase.ExecuteAsync(
            inviterUid,
            invitedFriendId,
            difficulty,
            expectedResult
        );

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().Be(1001);
        result.InviterPlayerId.Should().Be(1);
        result.InviterPlayerName.Should().Be("Inviter Player");
        result.InvitedPlayerId.Should().Be(invitedFriendId);
        result.InvitedPlayerName.Should().Be("Invited Friend");
        result.GameName.Should().Be("Inviter Player vs Invited Friend");
        result.Difficulty.Should().Be(difficulty);
        result.ExpectedResult.Should().Be(expectedResult);

        _mockPlayerRepository.Verify(r => r.GetByUidAsync(inviterUid), Times.Once);
        _mockPlayerRepository.Verify(r => r.GetByIdAsync(invitedFriendId), Times.Once);
        _mockGameRepository.Verify(r => r.UpdateAsync(It.Is<Game>(g => g.IsFromInvitation)), Times.Once);
        _mockInvitationRepository.Verify(r => r.CreateAsync(It.IsAny<GameInvitation>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidInviterUid_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidUid = "invalid-uid";
        var invitedFriendId = 2;

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(invalidUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sendGameInvitationUseCase.ExecuteAsync(invalidUid, invitedFriendId, "facil", "MAYOR")
        );

        _mockPlayerRepository.Verify(r => r.GetByUidAsync(invalidUid), Times.Once);
        _mockCreateGameUseCase.Verify(u => u.ExecuteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidInvitedFriendId_ShouldThrowNotFoundException()
    {
        // Arrange
        var inviterUid = "inviter-uid-123";
        var invalidFriendId = 999;

        var inviterProfile = new PlayerProfile
        {
            Id = 1,
            Name = "Inviter Player",
            Uid = inviterUid
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(inviterUid))
            .ReturnsAsync(inviterProfile);
        _mockPlayerRepository.Setup(r => r.GetByIdAsync(invalidFriendId))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sendGameInvitationUseCase.ExecuteAsync(inviterUid, invalidFriendId, "facil", "MAYOR")
        );

        _mockPlayerRepository.Verify(r => r.GetByIdAsync(invalidFriendId), Times.Once);
        _mockCreateGameUseCase.Verify(u => u.ExecuteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Theory]
    [InlineData("facil", "MAYOR")]
    [InlineData("medio", "MENOR")]
    [InlineData("dificil", "MAYOR")]
    public async Task ExecuteAsync_WithDifferentDifficulties_ShouldCreateInvitationWithCorrectParameters(
        string difficulty,
        string expectedResult)
    {
        // Arrange
        var inviterUid = "inviter-uid-123";
        var invitedFriendId = 2;

        var inviterProfile = new PlayerProfile { Id = 1, Name = "Player1", Uid = inviterUid };
        var invitedProfile = new PlayerProfile { Id = 2, Name = "Player2", Uid = "uid2" };
        var createdGame = new Game
        {
            Id = 1001,
            Name = "Player1 vs Player2",
            ExpectedResult = expectedResult,
            Questions = new List<Question>()
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(inviterUid)).ReturnsAsync(inviterProfile);
        _mockPlayerRepository.Setup(r => r.GetByIdAsync(invitedFriendId)).ReturnsAsync(invitedProfile);
        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            difficulty,
            expectedResult
        )).ReturnsAsync(createdGame);
        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);
        _mockInvitationRepository.Setup(r => r.CreateAsync(It.IsAny<GameInvitation>()))
            .ReturnsAsync((GameInvitation inv) => inv);

        // Act
        var result = await _sendGameInvitationUseCase.ExecuteAsync(
            inviterUid,
            invitedFriendId,
            difficulty,
            expectedResult
        );

        // Assert
        result.Difficulty.Should().Be(difficulty);
        result.ExpectedResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkGameAsFromInvitation()
    {
        // Arrange
        var inviterUid = "inviter-uid-123";
        var invitedFriendId = 2;
        var inviterProfile = new PlayerProfile { Id = 1, Name = "Player1", Uid = inviterUid };
        var invitedProfile = new PlayerProfile { Id = 2, Name = "Player2", Uid = "uid2" };
        var createdGame = new Game
        {
            Id = 1001,
            Name = "Player1 vs Player2",
            IsFromInvitation = false,
            Questions = new List<Question>()
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(inviterUid)).ReturnsAsync(inviterProfile);
        _mockPlayerRepository.Setup(r => r.GetByIdAsync(invitedFriendId)).ReturnsAsync(invitedProfile);
        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync(createdGame);
        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);
        _mockInvitationRepository.Setup(r => r.CreateAsync(It.IsAny<GameInvitation>()))
            .ReturnsAsync((GameInvitation inv) => inv);

        // Act
        await _sendGameInvitationUseCase.ExecuteAsync(inviterUid, invitedFriendId, "facil", "MAYOR");

        // Assert
        _mockGameRepository.Verify(r => r.UpdateAsync(
            It.Is<Game>(g => g.IsFromInvitation == true)
        ), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateCorrectGameName()
    {
        // Arrange
        var inviterUid = "inviter-uid-123";
        var invitedFriendId = 2;
        var inviterProfile = new PlayerProfile { Id = 1, Name = "Alice", Uid = inviterUid };
        var invitedProfile = new PlayerProfile { Id = 2, Name = "Bob", Uid = "uid2" };
        var createdGame = new Game
        {
            Id = 1001,
            Name = "Alice vs Bob",
            Questions = new List<Question>()
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(inviterUid)).ReturnsAsync(inviterProfile);
        _mockPlayerRepository.Setup(r => r.GetByIdAsync(invitedFriendId)).ReturnsAsync(invitedProfile);
        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(
            inviterUid,
            "Alice vs Bob",
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync(createdGame);
        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);
        _mockInvitationRepository.Setup(r => r.CreateAsync(It.IsAny<GameInvitation>()))
            .ReturnsAsync((GameInvitation inv) => inv);

        // Act
        var result = await _sendGameInvitationUseCase.ExecuteAsync(
            inviterUid,
            invitedFriendId,
            "facil",
            "MAYOR"
        );

        // Assert
        result.GameName.Should().Be("Alice vs Bob");
        _mockCreateGameUseCase.Verify(u => u.ExecuteAsync(
            inviterUid,
            "Alice vs Bob",
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);
    }
}