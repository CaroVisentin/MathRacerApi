using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de iniciar partida en modo infinito
/// </summary>
public class StartInfiniteGameUseCaseTests
{
    private readonly Mock<IInfiniteGameRepository> _mockInfiniteGameRepository;
    private readonly Mock<ILevelRepository> _mockLevelRepository;
    private readonly Mock<IWorldRepository> _mockWorldRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase; 
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase; 
    private readonly StartInfiniteGameUseCase _useCase;

    public StartInfiniteGameUseCaseTests()
    {
        _mockInfiniteGameRepository = new Mock<IInfiniteGameRepository>();
        _mockLevelRepository = new Mock<ILevelRepository>();
        _mockWorldRepository = new Mock<IWorldRepository>();
        _getQuestionsUseCase = new GetQuestionsUseCase(); 
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _getPlayerByIdUseCase = new GetPlayerByIdUseCase(
            _mockPlayerRepository.Object); 

        _useCase = new StartInfiniteGameUseCase(
            _mockInfiniteGameRepository.Object,
            _mockLevelRepository.Object,
            _mockWorldRepository.Object,
            _getQuestionsUseCase,
            _getPlayerByIdUseCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUid_ShouldCreateInfiniteGame()
    {
        // Arrange
        var uid = "test-uid-123";
        var player = CreateTestPlayer();
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        _mockPlayerRepository
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        _mockInfiniteGameRepository
            .Setup(x => x.AddAsync(It.IsAny<InfiniteGame>()))
            .ReturnsAsync((InfiniteGame g) => { g.Id = 1; return g; });

        // Act
        var result = await _useCase.ExecuteAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.PlayerUid.Should().Be(uid);
        result.PlayerName.Should().Be(player.Name);
        result.Questions.Should().HaveCount(9);
        result.CurrentBatch.Should().Be(0);
        result.CurrentWorldId.Should().Be(1);
        result.CorrectAnswers.Should().Be(0);
        result.IsActive.Should().BeTrue();
        result.AbandonedAt.Should().BeNull();

        _mockInfiniteGameRepository.Verify(x => x.AddAsync(It.IsAny<InfiniteGame>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidUid_ShouldThrowNotFoundException()
    {
        // Arrange
        var uid = "invalid-uid";

        _mockPlayerRepository
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(uid);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerate9QuestionsInFirstBatch()
    {
        // Arrange
        var uid = "test-uid";
        var player = CreateTestPlayer();
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        _mockPlayerRepository
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        _mockInfiniteGameRepository
            .Setup(x => x.AddAsync(It.IsAny<InfiniteGame>()))
            .ReturnsAsync((InfiniteGame g) => { g.Id = 1; return g; });

        // Act
        var result = await _useCase.ExecuteAsync(uid);

        // Assert
        result.Questions.Should().HaveCount(9); // 3 niveles × 3 preguntas
        result.Questions.Should().OnlyContain(q => !string.IsNullOrEmpty(q.Equation));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseLevels1_6_11FromWorld1()
    {
        // Arrange
        var uid = "test-uid";
        var player = CreateTestPlayer();
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        _mockPlayerRepository
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(1))
            .ReturnsAsync(levels);

        _mockInfiniteGameRepository
            .Setup(x => x.AddAsync(It.IsAny<InfiniteGame>()))
            .ReturnsAsync((InfiniteGame g) => { g.Id = 1; return g; });

        // Act
        var result = await _useCase.ExecuteAsync(uid);

        // Assert
        result.Questions.Should().HaveCount(9);
        _mockLevelRepository.Verify(x => x.GetAllByWorldIdAsync(1), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetGameStartedAtToUtcNow()
    {
        // Arrange
        var uid = "test-uid";
        var player = CreateTestPlayer();
        var worlds = CreateTestWorlds();
        var levels = CreateTestLevels();

        var beforeExecution = DateTime.UtcNow;

        _mockPlayerRepository
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(worlds);

        _mockLevelRepository
            .Setup(x => x.GetAllByWorldIdAsync(It.IsAny<int>()))
            .ReturnsAsync(levels);

        _mockInfiniteGameRepository
            .Setup(x => x.AddAsync(It.IsAny<InfiniteGame>()))
            .ReturnsAsync((InfiniteGame g) => { g.Id = 1; return g; });

        // Act
        var result = await _useCase.ExecuteAsync(uid);
        var afterExecution = DateTime.UtcNow;

        // Assert
        result.GameStartedAt.Should().BeOnOrAfter(beforeExecution);
        result.GameStartedAt.Should().BeOnOrBefore(afterExecution);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoWorldsAvailable_ShouldHandleGracefully()
    {
        // Arrange
        var uid = "test-uid";
        var player = CreateTestPlayer();
        var emptyWorlds = new List<World>();

        _mockPlayerRepository
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _mockWorldRepository
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(emptyWorlds);

        _mockInfiniteGameRepository
            .Setup(x => x.AddAsync(It.IsAny<InfiniteGame>()))
            .ReturnsAsync((InfiniteGame g) => { g.Id = 1; return g; });

        // Act
        var result = await _useCase.ExecuteAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Questions.Should().BeEmpty(); // Sin mundos, no hay preguntas
    }

    #region Helper Methods

    private PlayerProfile CreateTestPlayer()
    {
        return new PlayerProfile
        {
            Id = 1,
            Uid = "test-uid-123",
            Name = "Test Player",
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

    #endregion
}