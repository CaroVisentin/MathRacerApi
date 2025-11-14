using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de abandonar partida infinita
/// </summary>
public class AbandonInfiniteGameUseCaseTests
{
    private readonly Mock<IInfiniteGameRepository> _mockInfiniteGameRepository;
    private readonly AbandonInfiniteGameUseCase _useCase;

    public AbandonInfiniteGameUseCaseTests()
    {
        _mockInfiniteGameRepository = new Mock<IInfiniteGameRepository>();
        _useCase = new AbandonInfiniteGameUseCase(_mockInfiniteGameRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidGameId_ShouldAbandonGame()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        var beforeExecution = DateTime.UtcNow;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);
        var afterExecution = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.AbandonedAt.Should().NotBeNull();
        result.AbandonedAt.Should().BeOnOrAfter(beforeExecution);
        result.AbandonedAt.Should().BeOnOrBefore(afterExecution);
        result.IsActive.Should().BeFalse();

        _mockInfiniteGameRepository.Verify(x => x.UpdateAsync(It.Is<InfiniteGame>(g => 
            g.Id == gameId && 
            g.AbandonedAt != null
        )), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidGameId_ShouldThrowNotFoundException()
    {
        // Arrange
        var gameId = 999;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((InfiniteGame?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{gameId}*");
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyAbandonedGame_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.AbandonedAt = DateTime.UtcNow.AddMinutes(-5);

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*ya ha sido abandonada*");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreserveGameState()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CorrectAnswers = 10;
        game.CurrentBatch = 2;
        game.CurrentQuestionIndex = 5;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.CorrectAnswers.Should().Be(10);
        result.CurrentBatch.Should().Be(2);
        result.CurrentQuestionIndex.Should().Be(5);
        result.AbandonedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryOnce()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        await _useCase.ExecuteAsync(gameId);

        // Assert
        _mockInfiniteGameRepository.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _mockInfiniteGameRepository.Verify(x => x.UpdateAsync(It.IsAny<InfiniteGame>()), Times.Once);
    }

    #region Helper Methods

    private InfiniteGame CreateTestGame()
    {
        var questions = new List<InfiniteQuestion>();
        for (int i = 0; i < 9; i++)
        {
            questions.Add(new InfiniteQuestion
            {
                Id = i + 1,
                Equation = $"y = {i}*x + 1",
                Options = new List<int> { 1, 2, 3, 4 },
                CorrectAnswer = 2,
                ExpectedResult = i % 2 == 0 ? "MAYOR" : "MENOR"
            });
        }

        return new InfiniteGame
        {
            Id = 1,
            PlayerId = 1,
            PlayerUid = "test-uid",
            PlayerName = "Test Player",
            Questions = questions,
            CurrentBatch = 0,
            CurrentWorldId = 1,
            CurrentDifficultyStep = 0,
            CorrectAnswers = 0,
            CurrentQuestionIndex = 0,
            GameStartedAt = DateTime.UtcNow,
            AbandonedAt = null
        };
    }

    #endregion
}