using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de enviar respuesta en modo infinito
/// </summary>
public class SubmitInfiniteAnswerUseCaseTests
{
    private readonly Mock<IInfiniteGameRepository> _mockInfiniteGameRepository;
    private readonly SubmitInfiniteAnswerUseCase _useCase;

    public SubmitInfiniteAnswerUseCaseTests()
    {
        _mockInfiniteGameRepository = new Mock<IInfiniteGameRepository>();
        _useCase = new SubmitInfiniteAnswerUseCase(_mockInfiniteGameRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithCorrectAnswer_ShouldIncrementCorrectAnswers()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        var selectedAnswer = game.Questions[0].CorrectAnswer;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, selectedAnswer);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.TotalCorrectAnswers.Should().Be(1);
        result.CorrectAnswer.Should().Be(selectedAnswer);
        result.CurrentQuestionIndex.Should().Be(1);
        result.NeedsNewBatch.Should().BeFalse();

        _mockInfiniteGameRepository.Verify(x => x.UpdateAsync(It.Is<InfiniteGame>(g =>
            g.CorrectAnswers == 1 &&
            g.CurrentQuestionIndex == 1
        )), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithIncorrectAnswer_ShouldNotIncrementCorrectAnswers()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        var wrongAnswer = game.Questions[0].Options.First(o => o != game.Questions[0].CorrectAnswer);

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, wrongAnswer);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeFalse();
        result.TotalCorrectAnswers.Should().Be(0);
        result.CorrectAnswer.Should().Be(game.Questions[0].CorrectAnswer);
        result.CurrentQuestionIndex.Should().Be(1);

        _mockInfiniteGameRepository.Verify(x => x.UpdateAsync(It.Is<InfiniteGame>(g =>
            g.CorrectAnswers == 0 &&
            g.CurrentQuestionIndex == 1
        )), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_OnNinthQuestion_ShouldSetNeedsNewBatchToTrue()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CurrentQuestionIndex = 8; // Última pregunta del lote (índice 8)

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, game.Questions[8].CorrectAnswer);

        // Assert
        result.NeedsNewBatch.Should().BeTrue();
        result.CurrentQuestionIndex.Should().Be(9);
    }

    [Fact]
    public async Task ExecuteAsync_BeforeNinthQuestion_ShouldSetNeedsNewBatchToFalse()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CurrentQuestionIndex = 5;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, game.Questions[5].CorrectAnswer);

        // Assert
        result.NeedsNewBatch.Should().BeFalse();
        result.CurrentQuestionIndex.Should().Be(6);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidGameId_ShouldThrowNotFoundException()
    {
        // Arrange
        var gameId = 999;
        var selectedAnswer = 1;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((InfiniteGame?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId, selectedAnswer);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{gameId}*");
    }

    [Fact]
    public async Task ExecuteAsync_WithAbandonedGame_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.AbandonedAt = DateTime.UtcNow.AddMinutes(-5);

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId, 1);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*abandonada*");
    }

    [Fact]
    public async Task ExecuteAsync_WithQuestionIndexOutOfRange_ShouldThrowBusinessException()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CurrentQuestionIndex = 10; // Más allá del lote actual

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId, 1);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*No hay más preguntas disponibles*");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateLastAnswerTime()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        var beforeExecution = DateTime.UtcNow;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        await _useCase.ExecuteAsync(gameId, game.Questions[0].CorrectAnswer);
        var afterExecution = DateTime.UtcNow;

        // Assert
        _mockInfiniteGameRepository.Verify(x => x.UpdateAsync(It.Is<InfiniteGame>(g =>
            g.LastAnswerTime.HasValue &&
            g.LastAnswerTime.Value >= beforeExecution &&
            g.LastAnswerTime.Value <= afterExecution
        )), Times.Once);
    }

    [Theory]
    [InlineData(0, false)] // Primera pregunta
    [InlineData(4, false)] // Quinta pregunta
    [InlineData(8, true)]  // Novena pregunta
    public async Task ExecuteAsync_ShouldCorrectlyDetermineNeedsNewBatch(
        int currentIndex,
        bool expectedNeedsNewBatch)
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CurrentQuestionIndex = currentIndex;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, game.Questions[currentIndex].CorrectAnswer);

        // Assert
        result.NeedsNewBatch.Should().Be(expectedNeedsNewBatch);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleCorrectAnswers_ShouldAccumulateCorrectly()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CorrectAnswers = 5; // Ya tiene 5 respuestas correctas

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, game.Questions[0].CorrectAnswer);

        // Assert
        result.TotalCorrectAnswers.Should().Be(6);
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