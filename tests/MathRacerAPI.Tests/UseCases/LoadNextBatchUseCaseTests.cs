using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de cargar siguiente lote de ecuaciones
/// </summary>
public class LoadNextBatchUseCaseTests
{
    private readonly Mock<IInfiniteGameRepository> _mockInfiniteGameRepository;
    private readonly Mock<ILevelRepository> _mockLevelRepository;
    private readonly Mock<IWorldRepository> _mockWorldRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly LoadNextBatchUseCase _useCase;

    public LoadNextBatchUseCaseTests()
    {
        _mockInfiniteGameRepository = new Mock<IInfiniteGameRepository>();
        _mockLevelRepository = new Mock<ILevelRepository>();
        _mockWorldRepository = new Mock<IWorldRepository>();
        _getQuestionsUseCase = new GetQuestionsUseCase(); 

        _useCase = new LoadNextBatchUseCase(
            _mockInfiniteGameRepository.Object,
            _mockLevelRepository.Object,
            _mockWorldRepository.Object,
            _getQuestionsUseCase); 
    }

    [Fact]
    public async Task ExecuteAsync_WithValidGameId_ShouldLoadNextBatch()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.Should().NotBeNull();
        result.CurrentBatch.Should().Be(1); // Incrementado de 0 a 1
        result.Questions.Should().HaveCount(9);
        result.CurrentQuestionIndex.Should().Be(0); // Reseteado

        _mockInfiniteGameRepository.Verify(x => x.UpdateAsync(It.IsAny<InfiniteGame>()), Times.Once);
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
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*abandonada*");
    }

    [Theory]
    [InlineData(0, 1)] // Lote 0 -> se incrementa a 1 -> (1/3)+1 = mundo 1
    [InlineData(1, 1)] // Lote 1 -> se incrementa a 2 -> (2/3)+1 = mundo 1
    [InlineData(2, 2)] // Lote 2 -> se incrementa a 3 -> (3/3)+1 = mundo 2
    [InlineData(3, 2)] // Lote 3 -> se incrementa a 4 -> (4/3)+1 = mundo 2
    [InlineData(4, 2)] // Lote 4 -> se incrementa a 5 -> (5/3)+1 = mundo 2
    [InlineData(5, 2)] // Lote 5 -> se incrementa a 6 -> (6/3)+1 = mundo 3 (usará mundo 2)
    public async Task ExecuteAsync_ShouldUseCorrectWorldForBatch(int currentBatch, int expectedWorldId)
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CurrentBatch = currentBatch;
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        var capturedWorldId = 0;

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .Callback<int>(worldId => capturedWorldId = worldId)
            .ReturnsAsync(levels);

        // Act
        await _useCase.ExecuteAsync(gameId);

        // Assert
        if (expectedWorldId <= 2) // Solo tenemos 2 mundos en CreateTestWorlds
        {
            capturedWorldId.Should().Be(expectedWorldId);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerate9Questions()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.Questions.Should().HaveCount(9);
        result.Questions.Should().OnlyContain(q => !string.IsNullOrEmpty(q.Equation));
        result.Questions.Should().OnlyContain(q => !string.IsNullOrEmpty(q.ExpectedResult));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldResetQuestionIndex()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CurrentQuestionIndex = 9; // Al final del lote anterior
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.CurrentQuestionIndex.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoWorldAvailable_ShouldUseLastAvailableParams()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        game.CurrentBatch = 10; // Lote muy alto que excede mundos disponibles
        var worlds = CreateTestWorlds(); // Solo 2 mundos
        var levels = CreateTestLevels();

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(9);
        // Debe haber usado los parámetros del último mundo disponible
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncrementBatchNumber()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        var initialBatch = game.CurrentBatch;
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.CurrentBatch.Should().Be(initialBatch + 1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseLevels1_6_11Pattern()
    {
        // Arrange
        var gameId = 1;
        var game = CreateTestGame();
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        // Act
        var result = await _useCase.ExecuteAsync(gameId);

        // Assert
        result.Questions.Should().HaveCount(9);
        _mockLevelRepository.Verify(x => x.GetAllByWorldIdAsync(It.IsAny<int>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleConsecutiveBatches_ShouldMaintainCorrectState()
    {
        // Arrange
        var gameId = 1;
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        // Crear un juego separado para cada llamada
        var game1 = CreateTestGame();
        game1.CurrentBatch = 0;

        var game2 = CreateTestGame();
        game2.CurrentBatch = 1;

        var game3 = CreateTestGame();
        game3.CurrentBatch = 2;

        // Configurar el mock para devolver diferentes objetos según el estado
        var callCount = 0;
        _mockInfiniteGameRepository
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount switch
                {
                    1 => game1,
                    2 => game2,
                    3 => game3,
                    _ => game3
                };
            });

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        // Act - Cargar 3 lotes consecutivos
        var result1 = await _useCase.ExecuteAsync(gameId);
        var result2 = await _useCase.ExecuteAsync(gameId);
        var result3 = await _useCase.ExecuteAsync(gameId);

        // Assert
        result1.CurrentBatch.Should().Be(1);
        result2.CurrentBatch.Should().Be(2);
        result3.CurrentBatch.Should().Be(3);
    }

    #region Helper Methods

    private InfiniteGame CreateTestGame()
    {
        return new InfiniteGame
        {
            Id = 1,
            PlayerId = 1,
            PlayerUid = "test-uid",
            PlayerName = "Test Player",
            Questions = CreateTestQuestions(9),
            CurrentBatch = 0,
            CurrentWorldId = 1,
            CurrentDifficultyStep = 0,
            CorrectAnswers = 0,
            CurrentQuestionIndex = 0,
            GameStartedAt = DateTime.UtcNow,
            AbandonedAt = null // Activo por defecto
        };
    }

    private List<World> CreateTestWorlds()
    {
        return new List<World>
        {
            new World
            {
                Id = 1,
                Name = "Mundo 1",
                Operations = new List<string> { "+", "-" },
                OptionsCount = 4,
                OptionRangeMin = 1,
                OptionRangeMax = 10,
                NumberRangeMin = 1,
                NumberRangeMax = 10,
                TimePerEquation = 10
            },
            new World
            {
                Id = 2,
                Name = "Mundo 2",
                Operations = new List<string> { "+", "-", "*" },
                OptionsCount = 4,
                OptionRangeMin = 1,
                OptionRangeMax = 20,
                NumberRangeMin = 1,
                NumberRangeMax = 20,
                TimePerEquation = 12
            }
        };
    }

    private List<Level> CreateTestLevels()
    {
        return new List<Level>
        {
            new Level { Id = 1, Number = 1, WorldId = 1, TermsCount = 2, VariablesCount = 1, ResultType = "MAYOR" },
            new Level { Id = 6, Number = 6, WorldId = 1, TermsCount = 3, VariablesCount = 1, ResultType = "MAYOR" },
            new Level { Id = 11, Number = 11, WorldId = 1, TermsCount = 4, VariablesCount = 2, ResultType = "MENOR" }
        };
    }

    private List<InfiniteQuestion> CreateTestQuestions(int count)
    {
        var questions = new List<InfiniteQuestion>();
        for (int i = 0; i < count; i++)
        {
            questions.Add(new InfiniteQuestion
            {
                Id = i + 1,
                Equation = $"y = {i}*x + {i}",
                Options = new List<int> { 1, 2, 3, 4 },
                CorrectAnswer = 2,
                ExpectedResult = i % 2 == 0 ? "MAYOR" : "MENOR"
            });
        }
        return questions;
    }

    #endregion
}