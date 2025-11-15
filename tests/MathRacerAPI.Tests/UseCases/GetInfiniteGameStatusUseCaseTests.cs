using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de obtener estado de partida infinita
/// </summary>
public class GetInfiniteGameStatusUseCaseTests
{
    private readonly Mock<IInfiniteGameRepository> _mockInfiniteGameRepository;
    private readonly GetInfiniteGameStatusUseCase _useCase;

    public GetInfiniteGameStatusUseCaseTests()
    {
        _mockInfiniteGameRepository = new Mock<IInfiniteGameRepository>();
        _useCase = new GetInfiniteGameStatusUseCase(_mockInfiniteGameRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidGameId_ShouldReturnGame()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(gameId);
        result.PlayerName.Should().Be(game.PlayerName);
        result.CorrectAnswers.Should().Be(game.CorrectAnswers);
        result.CurrentBatch.Should().Be(game.CurrentBatch);
        result.IsActive.Should().BeTrue();
        result.AbandonedAt.Should().BeNull();
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
    public async Task ExecuteAsync_ShouldReturnCompleteGameState()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CorrectAnswers = 15;
        game.CurrentQuestionIndex = 5;
        game.CurrentBatch = 2;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.CorrectAnswers.Should().Be(15);
        result.CurrentQuestionIndex.Should().Be(5);
        result.CurrentBatch.Should().Be(2);
        result.Questions.Should().HaveCount(9);
    }

    [Theory]
    [InlineData(true)]  // Juego activo
    [InlineData(false)] // Juego abandonado
    public async Task ExecuteAsync_ShouldReturnGameWithCorrectActiveState(bool isActive)
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.AbandonedAt = isActive ? null : DateTime.UtcNow.AddMinutes(-5);

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.IsActive.Should().Be(isActive);
        if (isActive)
        {
            result.AbandonedAt.Should().BeNull();
        }
        else
        {
            result.AbandonedAt.Should().NotBeNull();
            result.AbandonedAt.Should().BeBefore(DateTime.UtcNow);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithAbandonedGame_ShouldReturnAbandonedState()
    {
        // Arrange
        var gameId = 1;
        var abandonedTime = DateTime.UtcNow.AddMinutes(-10);
        var game = CreateTestGame();
        game.AbandonedAt = abandonedTime;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.IsActive.Should().BeFalse();
        result.AbandonedAt.Should().Be(abandonedTime);
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
        _mockInfiniteGameRepository.Verify(
            x => x.GetByIdAsync(gameId),
            Times.Once);
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
            AbandonedAt = null // Activo por defecto
        };
    }

    #endregion
}