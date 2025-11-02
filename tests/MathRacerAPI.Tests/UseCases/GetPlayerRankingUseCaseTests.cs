using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

public class GetPlayerRankingUseCaseTests
{
    private readonly Mock<IRankingRepository> _rankingRepositoryMock;
    private readonly GetPlayerRankingUseCase _useCase;

    public GetPlayerRankingUseCaseTests()
    {
        _rankingRepositoryMock = new Mock<IRankingRepository>();
        _useCase = new GetPlayerRankingUseCase(_rankingRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnRepositoryResult()
    {
        // Arrange
        var playerId = 1;
        var expectedTop10 = new List<PlayerProfile>
        {
            new() { Id = 1, Name = "Player1", Points = 300 },
            new() { Id = 2, Name = "Player2", Points = 200 },
            new() { Id = 3, Name = "Player3", Points = 100 }
        };
        var expectedPosition = 5;

        _rankingRepositoryMock
            .Setup(r => r.GetTop10WithPlayerPositionAsync(playerId))
            .ReturnsAsync((expectedTop10, expectedPosition));

        // Act
        var (actualTop10, actualPosition) = await _useCase.ExecuteAsync(playerId);

        // Assert
        Assert.Equal(expectedTop10, actualTop10);
        Assert.Equal(expectedPosition, actualPosition);
        _rankingRepositoryMock.Verify(r => r.GetTop10WithPlayerPositionAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryWithCorrectPlayerId()
    {
        // Arrange
        var playerId = 42;
        var emptyTop10 = new List<PlayerProfile>();

        _rankingRepositoryMock
            .Setup(r => r.GetTop10WithPlayerPositionAsync(playerId))
            .ReturnsAsync((emptyTop10, 0));

        // Act
        await _useCase.ExecuteAsync(playerId);

        // Assert
        _rankingRepositoryMock.Verify(r => r.GetTop10WithPlayerPositionAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleEmptyResults()
    {
        // Arrange
        var playerId = 1;
        var emptyTop10 = new List<PlayerProfile>();
        var positionZero = 0;

        _rankingRepositoryMock
            .Setup(r => r.GetTop10WithPlayerPositionAsync(playerId))
            .ReturnsAsync((emptyTop10, positionZero));

        // Act
        var (actualTop10, actualPosition) = await _useCase.ExecuteAsync(playerId);

        // Assert
        Assert.Empty(actualTop10);
        Assert.Equal(0, actualPosition);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateRepositoryExceptions()
    {
        // Arrange
        var playerId = 1;
        var expectedException = new InvalidOperationException("Database error");

        _rankingRepositoryMock
            .Setup(r => r.GetTop10WithPlayerPositionAsync(playerId))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(playerId));
        
        Assert.Equal(expectedException.Message, actualException.Message);
    }
}