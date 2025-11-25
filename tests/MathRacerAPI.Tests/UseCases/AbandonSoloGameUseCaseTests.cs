using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de abandonar partida individual
/// </summary>
public class AbandonSoloGameUseCaseTests
{
    private readonly Mock<ISoloGameRepository> _soloGameRepositoryMock;
    private readonly Mock<IEnergyRepository> _energyRepositoryMock;
    private readonly AbandonSoloGameUseCase _useCase;

    public AbandonSoloGameUseCaseTests()
    {
        _soloGameRepositoryMock = new Mock<ISoloGameRepository>();
        _energyRepositoryMock = new Mock<IEnergyRepository>();
        _useCase = new AbandonSoloGameUseCase(
            _soloGameRepositoryMock.Object,
            _energyRepositoryMock.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteAsync_WithValidGameId_ShouldAbandonGameSuccessfully()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        var beforeExecution = DateTime.UtcNow;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, uid);
        var afterExecution = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(SoloGameStatus.PlayerLost);
        result.LivesRemaining.Should().Be(0);
        result.GameFinishedAt.Should().NotBeNull();
        result.GameFinishedAt.Should().BeOnOrAfter(beforeExecution);
        result.GameFinishedAt.Should().BeOnOrBefore(afterExecution);

        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(game.PlayerId), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.Is<SoloGame>(g =>
            g.Id == gameId &&
            g.Status == SoloGameStatus.PlayerLost &&
            g.LivesRemaining == 0 &&
            g.GameFinishedAt != null
        )), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreserveGameProgress()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        game.PlayerPosition = 5;
        game.CorrectAnswers = 4;
        game.CurrentQuestionIndex = 5;
        game.MachinePosition = 3;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        result.PlayerPosition.Should().Be(5);
        result.CorrectAnswers.Should().Be(4);
        result.CurrentQuestionIndex.Should().Be(5);
        result.MachinePosition.Should().Be(3);
        result.Status.Should().Be(SoloGameStatus.PlayerLost);
        result.LivesRemaining.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoriesInCorrectOrder()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        var callOrder = new List<string>();

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .Callback(() => callOrder.Add("GetGame"))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .Callback(() => callOrder.Add("ConsumeEnergy"))
            .ReturnsAsync(true);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Callback(() => callOrder.Add("UpdateGame"))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        callOrder.Should().ContainInOrder("GetGame", "ConsumeEnergy", "UpdateGame");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ExecuteAsync_WithNonExistentGame_ShouldThrowNotFoundException()
    {
        // Arrange
        const int gameId = 999;
        const string uid = "test-uid-123";

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((SoloGame?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{gameId}*");

        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(It.IsAny<int>()), Times.Never);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnauthorizedPlayer_ShouldThrowBusinessException()
    {
        // Arrange
        const int gameId = 1;
        const string ownerUid = "owner-uid";
        const string otherUid = "other-uid";
        var game = CreateTestGame(ownerUid);

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId, otherUid);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*No tienes permiso*");

        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(It.IsAny<int>()), Times.Never);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    [Theory]
    [InlineData(SoloGameStatus.PlayerWon)]
    [InlineData(SoloGameStatus.PlayerLost)]
    [InlineData(SoloGameStatus.MachineWon)]
    public async Task ExecuteAsync_WithFinishedGame_ShouldThrowBusinessException(SoloGameStatus status)
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        game.Status = status;
        game.GameFinishedAt = DateTime.UtcNow.AddMinutes(-5);

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*ya finalizó*");

        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(It.IsAny<int>()), Times.Never);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ExecuteAsync_WithGameAtStartState_ShouldAbandonSuccessfully()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        game.PlayerPosition = 0;
        game.CorrectAnswers = 0;
        game.CurrentQuestionIndex = 0;
        game.LivesRemaining = 3;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Status.Should().Be(SoloGameStatus.PlayerLost);
        result.LivesRemaining.Should().Be(0);
        result.GameFinishedAt.Should().NotBeNull();

        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(game.PlayerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithGameNearCompletion_ShouldAbandonSuccessfully()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        game.PlayerPosition = 9;
        game.CorrectAnswers = 9;
        game.CurrentQuestionIndex = 9;
        game.LivesRemaining = 2;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Status.Should().Be(SoloGameStatus.PlayerLost);
        result.LivesRemaining.Should().Be(0);
        result.GameFinishedAt.Should().NotBeNull();

        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(game.PlayerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveWildcards_ShouldPreserveWildcardState()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        game.HasDoubleProgressActive = true;
        game.UsedWildcardTypes = new HashSet<int> { 1, 2 };
        game.ModifiedOptions = new List<int> { 1, 5, 7 };

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        result.HasDoubleProgressActive.Should().BeTrue();
        result.UsedWildcardTypes.Should().BeEquivalentTo(new HashSet<int> { 1, 2 });
        result.ModifiedOptions.Should().BeEquivalentTo(new List<int> { 1, 5, 7 });
    }

    [Fact]
    public async Task ExecuteAsync_WithMachineAhead_ShouldStillAbandon()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        game.PlayerPosition = 3;
        game.MachinePosition = 8;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Status.Should().Be(SoloGameStatus.PlayerLost);
        result.MachinePosition.Should().Be(8); // Se preserva la posición de la máquina
    }

    [Fact]
    public async Task ExecuteAsync_WithOnlyOneLifeRemaining_ShouldSetLivesToZero()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        game.LivesRemaining = 1;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        result.LivesRemaining.Should().Be(0);
        result.Status.Should().Be(SoloGameStatus.PlayerLost);
    }

    #endregion

    #region Integration with Energy System

    [Fact]
    public async Task ExecuteAsync_ShouldCallConsumeEnergyWithCorrectPlayerId()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        const int playerId = 42;
        var game = CreateTestGame(uid);
        game.PlayerId = playerId;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(playerId))
            .ReturnsAsync(true);

        // Act
        await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldConsumeEnergyBeforeUpdatingGame()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);
        var callOrder = new List<string>();

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .Callback(() => callOrder.Add("ConsumeEnergy"))
            .ReturnsAsync(true);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Callback(() => callOrder.Add("UpdateGame"))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        callOrder.Should().ContainInOrder("ConsumeEnergy", "UpdateGame");
    }

    #endregion

    #region Multiple Executions

    [Fact]
    public async Task ExecuteAsync_CalledMultipleTimes_ShouldOnlyAllowFirstExecution()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestGame(uid);

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act - Primera ejecución
        await _useCase.ExecuteAsync(gameId, uid);

        // Actualizar el mock para que devuelva el juego abandonado
        game.Status = SoloGameStatus.PlayerLost;
        game.GameFinishedAt = DateTime.UtcNow;

        // Act - Segunda ejecución
        Func<Task> act = async () => await _useCase.ExecuteAsync(gameId, uid);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*ya finalizó*");

        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(game.PlayerId), Times.Once);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Crea un juego de prueba en estado InProgress
    /// </summary>
    private SoloGame CreateTestGame(string uid)
    {
        var questions = new List<Question>();
        for (int i = 0; i < 15; i++)
        {
            questions.Add(new Question
            {
                Equation = $"2*{i} + 5 = ?",
                Options = new List<int> { i, i + 1, i + 2, i + 3 },
                CorrectAnswer = 2 * i + 5
            });
        }

        return new SoloGame
        {
            Id = 1,
            PlayerId = 1,
            PlayerUid = uid,
            PlayerName = "TestPlayer",
            LevelId = 1,
            WorldId = 1,
            ResultType = "VALOR",
            PlayerPosition = 0,
            LivesRemaining = 3,
            CorrectAnswers = 0,
            CurrentQuestionIndex = 0,
            MachinePosition = 0,
            Questions = questions,
            TotalQuestions = 10,
            TimePerEquation = 10,
            GameStartedAt = DateTime.UtcNow,
            Status = SoloGameStatus.InProgress,
            ReviewTimeSeconds = 3,
            PlayerProducts = new List<PlayerProduct>
            {
                new() { ProductId = 1, Name = "Auto 1", ProductTypeName = "AUTO" },
                new() { ProductId = 2, Name = "Personaje 1", ProductTypeName = "PERSONAJE" },
                new() { ProductId = 3, Name = "Fondo 1", ProductTypeName = "FONDO" }
            },
            MachineProducts = new List<PlayerProduct>
            {
                new() { ProductId = 4, Name = "Auto 2", ProductTypeName = "AUTO" },
                new() { ProductId = 5, Name = "Personaje 2", ProductTypeName = "PERSONAJE" },
                new() { ProductId = 6, Name = "Fondo 2", ProductTypeName = "FONDO" }
            },
            AvailableWildcards = new List<PlayerWildcard>(),
            UsedWildcardTypes = new HashSet<int>(),
            HasDoubleProgressActive = false
        };
    }

    #endregion
}