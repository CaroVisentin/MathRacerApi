using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de envío de respuestas en modo individual
/// </summary>
public class SubmitSoloAnswerUseCaseTests
{
    private readonly Mock<ISoloGameRepository> _soloGameRepositoryMock;
    private readonly Mock<IEnergyRepository> _energyRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly Mock<ILevelRepository> _levelRepositoryMock;
    private readonly SubmitSoloAnswerUseCase _submitSoloAnswerUseCase;

    public SubmitSoloAnswerUseCaseTests()
    {
        _soloGameRepositoryMock = new Mock<ISoloGameRepository>();
        _energyRepositoryMock = new Mock<IEnergyRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _levelRepositoryMock = new Mock<ILevelRepository>();

        var grantLevelRewardUseCase = new GrantLevelRewardUseCase(
            _playerRepositoryMock.Object);

        _submitSoloAnswerUseCase = new SubmitSoloAnswerUseCase(
            _soloGameRepositoryMock.Object,
            _energyRepositoryMock.Object,
            grantLevelRewardUseCase,
            _levelRepositoryMock.Object,
            _playerRepositoryMock.Object);

        SetupDefaultPlayerRepositoryMocks();
    }

    /// <summary>
    /// Configura mocks por defecto para el PlayerRepository
    /// </summary>
    private void SetupDefaultPlayerRepositoryMocks()
    {
        // Mock para GetByIdAsync - retorna un jugador de prueba
        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new PlayerProfile
            {
                Id = id,
                Uid = "test-uid-123",
                Name = "TestPlayer",
                Email = "test@test.com",
                LastLevelId = 0,
                Coins = 100,
                Points = 50
            });

        // Mock para AddCoinsAsync
        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Mock para UpdateLastLevelAsync
        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
    }

    #region Game Validation Tests

    [Fact]
    public async Task ExecuteAsync_WhenGameDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        const int gameId = 999;
        const int answer = 5;
        const string uid = "test-uid-123";

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((SoloGame?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid));

        exception.Message.Should().Contain($"Partida con ID {gameId}");
        
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameIsFinished_ShouldThrowBusinessException()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Status = SoloGameStatus.PlayerWon;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid));

        exception.Message.Should().Contain("partida ya finalizó");
        
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnauthorizedPlayer_ShouldThrowBusinessException()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string correctUid = "correct-uid";
        const string wrongUid = "wrong-uid";
        
        var game = CreateTestSoloGame(correctUid);

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, wrongUid));

        exception.Message.Should().Contain("No tienes permiso");
        
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoQuestionsAvailable_ShouldThrowBusinessException()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.CurrentQuestionIndex = game.Questions.Count; // Ya respondió todas

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid));

        exception.Message.Should().Contain("No hay más preguntas disponibles");
    }

    #endregion

    #region Correct Answer Tests

    [Fact]
    public async Task ExecuteAsync_WhenAnswerIsCorrect_ShouldIncreasePlayerPosition()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        var initialPosition = game.PlayerPosition;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.CorrectAnswer.Should().Be(correctAnswer);
        result.PlayerAnswer.Should().Be(correctAnswer);
        result.Game.PlayerPosition.Should().Be(initialPosition + 1);
        result.Game.CorrectAnswers.Should().Be(1);
        result.Game.CurrentQuestionIndex.Should().Be(1);
        
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCorrectAnswerAndPlayerWins_ShouldSetPlayerWonStatus()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = game.TotalQuestions - 1; // A punto de ganar

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(game.LevelId))
            .ReturnsAsync(new Level { Id = game.LevelId, WorldId = game.WorldId, Number = 5 });

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.Game.PlayerPosition.Should().Be(game.TotalQuestions);
        result.Game.GameFinishedAt.Should().NotBeNull();
        
        _playerRepositoryMock.Verify(x => x.GetByIdAsync(game.PlayerId), Times.Exactly(2)); 
        _playerRepositoryMock.Verify(x => x.AddCoinsAsync(game.PlayerId, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCorrectAnswer_ShouldNotLoseLife()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        var initialLives = game.LivesRemaining;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Game.LivesRemaining.Should().Be(initialLives);
    }

    #endregion

    #region Incorrect Answer Tests

    [Fact]
    public async Task ExecuteAsync_WhenAnswerIsIncorrect_ShouldLoseLife()
    {
        // Arrange
        const int gameId = 1;
        const int incorrectAnswer = 3;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        var initialLives = game.LivesRemaining;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, incorrectAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeFalse();
        result.CorrectAnswer.Should().Be(correctAnswer);
        result.PlayerAnswer.Should().Be(incorrectAnswer);
        result.Game.LivesRemaining.Should().Be(initialLives - 1);
        result.Game.PlayerPosition.Should().Be(0); // No avanza
        result.Game.CorrectAnswers.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIncorrectAnswerAndLastLife_ShouldSetPlayerLostStatus()
    {
        // Arrange
        const int gameId = 1;
        const int incorrectAnswer = 3;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.LivesRemaining = 1; // Última vida

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true); 

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, incorrectAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeFalse();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerLost);
        result.Game.LivesRemaining.Should().Be(0);
        result.Game.GameFinishedAt.Should().NotBeNull();
        
        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(game.PlayerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIncorrectAnswer_ShouldNotIncreasePlayerPosition()
    {
        // Arrange
        const int gameId = 1;
        const int incorrectAnswer = 3;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, incorrectAnswer, uid);

        // Assert
        result.Game.PlayerPosition.Should().Be(0);
        result.Game.CorrectAnswers.Should().Be(0);
    }

    #endregion

    #region World Completion Chest Tests

    [Fact]
    public async Task ExecuteAsync_WhenPlayerWinsLevel15_ShouldSetShouldOpenChestToTrue()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        const int levelId = 15;
        
        var game = CreateTestSoloGame(uid);
        game.LevelId = levelId;
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = game.TotalQuestions - 1; // A punto de ganar

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Mock: Nivel 15 del mundo 1
        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(new Level 
            { 
                Id = levelId, 
                WorldId = 1, 
                Number = 15 // Último nivel del mundo
            });

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.ShouldOpenWorldCompletionChest.Should().BeTrue();
        
        _levelRepositoryMock.Verify(x => x.GetByIdAsync(levelId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerWinsNonLevel15_ShouldSetShouldOpenChestToFalse()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        const int levelId = 10;
        
        var game = CreateTestSoloGame(uid);
        game.LevelId = levelId;
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = game.TotalQuestions - 1; // A punto de ganar

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Mock: Nivel 10 del mundo 1 (no es el último)
        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(new Level 
            { 
                Id = levelId, 
                WorldId = 1, 
                Number = 10 // No es el nivel 15
            });

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.ShouldOpenWorldCompletionChest.Should().BeFalse();
        
        _levelRepositoryMock.Verify(x => x.GetByIdAsync(levelId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerWinsButLevelNotFound_ShouldSetShouldOpenChestToFalse()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        const int levelId = 15;
        
        var game = CreateTestSoloGame(uid);
        game.LevelId = levelId;
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = game.TotalQuestions - 1; // A punto de ganar

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Mock: Nivel no encontrado
        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync((Level?)null);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.ShouldOpenWorldCompletionChest.Should().BeFalse();
        
        _levelRepositoryMock.Verify(x => x.GetByIdAsync(levelId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerDoesNotWin_ShouldNotCheckLevel()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = 5; // No está a punto de ganar

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.InProgress);
        result.ShouldOpenWorldCompletionChest.Should().BeFalse();
        
        // NO debe consultar el nivel porque no ganó
        _levelRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerLoses_ShouldNotCheckLevel()
    {
        // Arrange
        const int gameId = 1;
        const int incorrectAnswer = 3;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.LivesRemaining = 1; // Última vida

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, incorrectAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeFalse();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerLost);
        result.ShouldOpenWorldCompletionChest.Should().BeFalse();
        
        // NO debe consultar el nivel porque perdió
        _levelRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(1, 15, true)]   // Mundo 1, Nivel 15 → Debe abrir cofre
    [InlineData(2, 15, true)]   // Mundo 2, Nivel 15 → Debe abrir cofre
    [InlineData(1, 14, false)]  // Mundo 1, Nivel 14 → No debe abrir cofre
    [InlineData(1, 1, false)]   // Mundo 1, Nivel 1 → No debe abrir cofre
    [InlineData(3, 15, true)]   // Mundo 3, Nivel 15 → Debe abrir cofre
    public async Task ExecuteAsync_WithDifferentWorldsAndLevels_ShouldSetCorrectChestFlag(
        int worldId, int levelNumber, bool expectedShouldOpenChest)
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        const int levelId = 99;
        
        var game = CreateTestSoloGame(uid);
        game.LevelId = levelId;
        game.WorldId = worldId;
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = game.TotalQuestions - 1; // A punto de ganar

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(new Level 
            { 
                Id = levelId, 
                WorldId = worldId, 
                Number = levelNumber
            });

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.ShouldOpenWorldCompletionChest.Should().Be(expectedShouldOpenChest);
    }

    #endregion

    #region Machine Position Tests

    [Fact]
    public async Task ExecuteAsync_WhenAnswered_ShouldUpdateMachinePosition()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = answer;
        game.GameStartedAt = DateTime.UtcNow.AddSeconds(-50); // 50 segundos transcurridos
        var initialMachinePosition = game.MachinePosition;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        // Assert
        result.Game.MachinePosition.Should().BeGreaterThan(initialMachinePosition);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMachineReachesEnd_ShouldSetMachineWonStatus()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = answer;
        game.GameStartedAt = DateTime.UtcNow.AddSeconds(-500); // Mucho tiempo
        game.MachinePosition = 0;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        // Assert
        result.Game.Status.Should().Be(SoloGameStatus.MachineWon);
        result.Game.MachinePosition.Should().BeGreaterOrEqualTo(game.TotalQuestions);
        result.Game.GameFinishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMachineWins_ShouldNotConsumeEnergy()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = answer;
        game.GameStartedAt = DateTime.UtcNow.AddSeconds(-500);

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        // Assert
        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Time Management Tests

    [Fact]
    public async Task ExecuteAsync_WhenAnswered_ShouldSetLastAnswerTime()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        var beforeAnswer = DateTime.UtcNow;
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = answer;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        // Assert
        result.Game.LastAnswerTime.Should().NotBeNull();
        result.Game.LastAnswerTime.Should().BeCloseTo(beforeAnswer, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ExecuteAsync_WhenAnswered_ShouldIncrementQuestionIndex()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = answer;
        var initialIndex = game.CurrentQuestionIndex;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        // Assert
        result.Game.CurrentQuestionIndex.Should().Be(initialIndex + 1);
    }

    #endregion

    #region Timeout Validation Tests

    [Fact]
    public async Task ExecuteAsync_WhenTimeoutOnFirstQuestion_ShouldTreatAsIncorrect()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.LastAnswerTime = null; // Primera pregunta
        game.GameStartedAt = DateTime.UtcNow.AddSeconds(-50); // Timeout: 50 > 10 segundos

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.IsCorrect.Should().BeFalse(); // Timeout se trata como incorrecto
        result.Game.LivesRemaining.Should().Be(2); // Pierde vida por timeout
    }

    [Fact]
    public async Task ExecuteAsync_WhenTimeoutAfterReviewTime_ShouldTreatAsIncorrect()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.LastAnswerTime = DateTime.UtcNow.AddSeconds(-20); // 20 segundos desde última respuesta
        game.TimePerEquation = 10;
        game.ReviewTimeSeconds = 3;
        // Total permitido: 10 + 3 = 13 segundos, pero pasaron 20

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.IsCorrect.Should().BeFalse(); // Timeout
        result.Game.LivesRemaining.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWithinTimeLimit_ShouldProcessNormally()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.LastAnswerTime = DateTime.UtcNow.AddSeconds(-5); // 5 segundos
        game.TimePerEquation = 10;
        game.ReviewTimeSeconds = 3;
        // Total: 5 < 13 (dentro del límite)

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.IsCorrect.Should().BeTrue(); // Dentro del tiempo
        result.Game.LivesRemaining.Should().Be(3); // No pierde vida
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteAsync_WhenValidAnswer_ShouldCallUpdateRepository()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = answer;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        // Assert
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerLosesAllLives_ShouldConsumeEnergy()
    {
        // Arrange
        const int gameId = 1;
        const int incorrectAnswer = 3;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.LivesRemaining = 1;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _energyRepositoryMock
            .Setup(x => x.ConsumeEnergyAsync(game.PlayerId))
            .ReturnsAsync(true); 

        // Act
        await _submitSoloAnswerUseCase.ExecuteAsync(gameId, incorrectAnswer, uid);

        // Assert
        _energyRepositoryMock.Verify(x => x.ConsumeEnergyAsync(game.PlayerId), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(5, true)]    // Respuesta correcta
    [InlineData(3, false)]   // Respuesta incorrecta
    [InlineData(-1, false)]  // Respuesta negativa
    [InlineData(100, false)] // Respuesta fuera de rango
    public async Task ExecuteAsync_WithDifferentAnswers_ShouldProcessCorrectly(int answer, bool expectedCorrect)
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = 5;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        // Assert
        result.IsCorrect.Should().Be(expectedCorrect);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleCorrectAnswers_ShouldAccumulateProgress()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Questions[0].CorrectAnswer = 5;
        game.Questions[1].CorrectAnswer = 7;

        // Devolver siempre el objeto actualizado
        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(() => game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act - Primera respuesta correcta
        var result1 = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, 5, uid);
        
        // Act - Segunda respuesta correcta
        var result2 = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, 7, uid);

        // Assert
        result1.IsCorrect.Should().BeTrue();
        result2.IsCorrect.Should().BeTrue();
        result2.Game.CorrectAnswers.Should().Be(2);
        result2.Game.PlayerPosition.Should().Be(2);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Crea un SoloGame de prueba
    /// </summary>
    private static SoloGame CreateTestSoloGame(string uid)
    {
        return new SoloGame
        {
            Id = 1,
            PlayerId = 100,
            PlayerUid = uid,
            PlayerName = "TestPlayer",
            LevelId = 1,
            WorldId = 1,
            PlayerPosition = 0,
            LivesRemaining = 3,
            CorrectAnswers = 0,
            CurrentQuestionIndex = 0,
            MachinePosition = 0,
            Questions = new List<Question>
            {
                new Question
                {
                    Id = 1,
                    Equation = "y = 2*x + 3",
                    CorrectAnswer = 5,
                    Options = new List<int> { 3, 5, 7, 9 }
                },
                new Question
                {
                    Id = 2,
                    Equation = "y = x - 1",
                    CorrectAnswer = 4,
                    Options = new List<int> { 2, 4, 6, 8 }
                },
                new Question
                {
                    Id = 3,
                    Equation = "y = 3*x",
                    CorrectAnswer = 6,
                    Options = new List<int> { 3, 6, 9, 12 }
                }
            },
            TotalQuestions = 10,
            TimePerEquation = 10,
            GameStartedAt = DateTime.UtcNow,
            LastAnswerTime = null,
            ReviewTimeSeconds = 3,
            Status = SoloGameStatus.InProgress,
            PlayerProducts = new List<PlayerProduct>
            {
                new PlayerProduct { ProductId = 1, Name = "Auto", ProductTypeId = 1 }
            },
            MachineProducts = new List<PlayerProduct>
            {
                new PlayerProduct { ProductId = 2, Name = "Auto", ProductTypeId = 1 }
            }
        };
    }

    #endregion

    #region Tests for World Completion Chest Conditions

    [Fact]
    public async Task ExecuteAsync_WhenPlayerRepeatsLevel15_ShouldNotOpenChest()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        const int levelId = 15;
        
        var player = new PlayerProfile
        {
            Id = 100,
            Uid = uid,
            Name = "TestPlayer",
            Email = "test@test.com",
            LastLevelId = 15 // Ya completó el nivel 15
        };

        var game = CreateTestSoloGame(uid);
        game.LevelId = levelId;
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = game.TotalQuestions - 1;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(game.PlayerId))
            .ReturnsAsync(player);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(new Level 
            { 
                Id = levelId, 
                WorldId = 1, 
                Number = 15
            });

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.ShouldOpenWorldCompletionChest.Should().BeFalse(); // No abre cofre
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerCompletesNewLevel15_ShouldOpenChest()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        const int levelId = 15;
        
        var player = new PlayerProfile
        {
            Id = 100,
            Uid = uid,
            Name = "TestPlayer",
            Email = "test@test.com",
            LastLevelId = 14 // Está en el nivel 14, completará el 15 por primera vez
        };

        var game = CreateTestSoloGame(uid);
        game.LevelId = levelId;
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = game.TotalQuestions - 1;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(game.PlayerId))
            .ReturnsAsync(player);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(new Level 
            { 
                Id = levelId, 
                WorldId = 1, 
                Number = 15
            });

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.ShouldOpenWorldCompletionChest.Should().BeTrue(); // Abre cofre
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerCompletesLevel30FirstTime_ShouldOpenChest()
    {
        // Arrange
        const int gameId = 1;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        const int levelId = 30; // Nivel 15 del mundo 2
        
        var player = new PlayerProfile
        {
            Id = 100,
            Uid = uid,
            Name = "TestPlayer",
            Email = "test@test.com",
            LastLevelId = 29 // Nivel anterior
        };

        var game = CreateTestSoloGame(uid);
        game.LevelId = levelId;
        game.Questions[0].CorrectAnswer = correctAnswer;
        game.PlayerPosition = game.TotalQuestions - 1;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(game.PlayerId))
            .ReturnsAsync(player);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(new Level 
            { 
                Id = levelId, 
                WorldId = 2, 
                Number = 15 // Último nivel del mundo 2
            });

        // Act
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, correctAnswer, uid);

        // Assert
        result.Should().NotBeNull();
        result.IsCorrect.Should().BeTrue();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.ShouldOpenWorldCompletionChest.Should().BeTrue(); // Abre cofre
    }

    #endregion
}