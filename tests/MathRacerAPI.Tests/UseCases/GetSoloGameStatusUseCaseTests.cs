using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de obtención del estado de partida individual
/// </summary>
public class GetSoloGameStatusUseCaseTests
{
    private readonly Mock<ISoloGameRepository> _soloGameRepositoryMock;
    private readonly GetSoloGameStatusUseCase _getSoloGameStatusUseCase;

    public GetSoloGameStatusUseCaseTests()
    {
        _soloGameRepositoryMock = new Mock<ISoloGameRepository>();
        _getSoloGameStatusUseCase = new GetSoloGameStatusUseCase(_soloGameRepositoryMock.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteAsync_WhenValidRequest_ShouldReturnGameStatusWithUpdatedMachinePosition()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.Status = SoloGameStatus.InProgress;
        game.GameStartedAt = DateTime.UtcNow.AddSeconds(-30); // 30 segundos de juego

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Should().NotBeNull();
        result.Game.Should().NotBeNull();
        result.Game.Id.Should().Be(gameId);
        result.ElapsedTime.Should().BeGreaterOrEqualTo(30);
        
        // La máquina debería haber avanzado
        result.Game.MachinePosition.Should().BeGreaterThan(0);
        
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameInProgress_ShouldCalculateCorrectElapsedTime()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.GameStartedAt = DateTime.UtcNow.AddMinutes(-2); // 2 minutos de juego

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.ElapsedTime.Should().BeGreaterOrEqualTo(120); // Al menos 120 segundos
        result.ElapsedTime.Should().BeLessThan(125); // Menos de 125 segundos (margen de error)
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameFinished_ShouldNotUpdateMachinePosition()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.Status = SoloGameStatus.PlayerWon;
        game.GameFinishedAt = DateTime.UtcNow;
        var originalMachinePosition = game.MachinePosition;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Should().NotBeNull();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        result.Game.MachinePosition.Should().Be(originalMachinePosition); // No debería cambiar
        
        // No debería llamar a Update porque el juego ya terminó
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoUidProvided_ShouldReturnGameStatus()
    {
        // Arrange
        const int gameId = 1;
        var game = CreateTestSoloGame("any-uid");
        game.Id = gameId;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, requestingPlayerUid: null);

        // Assert
        result.Should().NotBeNull();
        result.Game.Id.Should().Be(gameId);
    }

    #endregion

    #region Validation Tests - Game Not Found

    [Fact]
    public async Task ExecuteAsync_WhenGameNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const int gameId = 999;
        const string uid = "test-uid-123";

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((SoloGame?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid));

        exception.Message.Should().Contain($"Partida con ID {gameId}");
        
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    #endregion

    #region Validation Tests - Unauthorized Access

    [Fact]
    public async Task ExecuteAsync_WhenUnauthorizedPlayer_ShouldThrowBusinessException()
    {
        // Arrange
        const int gameId = 1;
        const string correctUid = "correct-uid";
        const string wrongUid = "wrong-uid";
        
        var game = CreateTestSoloGame(correctUid);
        game.Id = gameId;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _getSoloGameStatusUseCase.ExecuteAsync(gameId, wrongUid));

        exception.Message.Should().Contain("No tienes permiso");
        
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    #endregion

    #region Validation Tests - Review Time

    [Fact]
    public async Task ExecuteAsync_WhenCalledBeforeReviewTimeExpires_ShouldThrowValidationException()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.LastAnswerTime = DateTime.UtcNow; // Acaba de responder
        game.ReviewTimeSeconds = 3;
        game.Status = SoloGameStatus.InProgress;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid));

        exception.Message.Should().Contain("Debes esperar");
        exception.Message.Should().Contain("segundos más");
        
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledAfterReviewTimeExpires_ShouldReturnGameStatus()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.LastAnswerTime = DateTime.UtcNow.AddSeconds(-5); // Hace 5 segundos
        game.ReviewTimeSeconds = 3; // Solo necesita esperar 3
        game.Status = SoloGameStatus.InProgress;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Should().NotBeNull();
        result.Game.Id.Should().Be(gameId);
        
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFirstQuestion_ShouldNotValidateReviewTime()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.LastAnswerTime = null; // Primera pregunta, no ha respondido nada
        game.Status = SoloGameStatus.InProgress;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Should().NotBeNull();
        result.Game.Id.Should().Be(gameId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameFinished_ShouldNotValidateReviewTime()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.LastAnswerTime = DateTime.UtcNow; // Justo acaba de responder
        game.ReviewTimeSeconds = 3;
        game.Status = SoloGameStatus.PlayerWon; // Pero el juego ya terminó

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Should().NotBeNull();
        result.Game.Status.Should().Be(SoloGameStatus.PlayerWon);
        
        // No debería lanzar ValidationException
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
    }

    #endregion

    #region Machine Position Update Tests

    [Fact]
    public async Task ExecuteAsync_WhenMachinePositionShouldUpdate_ShouldCalculateCorrectPosition()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.TotalQuestions = 10;
        game.TimePerEquation = 10;
        game.GameStartedAt = DateTime.UtcNow.AddSeconds(-100); // 100 segundos transcurridos
        game.Status = SoloGameStatus.InProgress;
        game.MachinePosition = 0;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Game.MachinePosition.Should().BeGreaterThan(0);
        // Con TotalEstimatedTime = (10 + 3) * 10 = 130 segundos
        // Progress = 100 / 130 ≈ 0.77
        // MachinePosition = (int)(0.77 * 10) ≈ 7
        result.Game.MachinePosition.Should().BeInRange(6, 8);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMachineReachesEnd_ShouldNotExceedTotalQuestions()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.TotalQuestions = 10;
        game.TimePerEquation = 10;
        game.GameStartedAt = DateTime.UtcNow.AddSeconds(-500); // Mucho tiempo transcurrido
        game.Status = SoloGameStatus.InProgress;
        game.MachinePosition = 0;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Game.MachinePosition.Should().Be(10); // No debe exceder TotalQuestions
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteAsync_WhenGameInProgress_ShouldCallUpdateRepository()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.Status = SoloGameStatus.InProgress;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleCalls_ShouldUpdateMachineProgressively()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.Status = SoloGameStatus.InProgress;
        game.TotalQuestions = 10;
        game.TimePerEquation = 10;
        game.GameStartedAt = DateTime.UtcNow.AddSeconds(-30); // Comienza con 30 segundos

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act - Primera llamada
        var result1 = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);
        
        // Simular paso del tiempo real (1 segundo)
        await Task.Delay(1000); // Espera 1 segundo
        
        // Act - Segunda llamada
        var result2 = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result2.Game.MachinePosition.Should().BeGreaterOrEqualTo(result1.Game.MachinePosition);
        result2.ElapsedTime.Should().BeGreaterThan(result1.ElapsedTime);
        
        _soloGameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Exactly(2));
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Exactly(2));
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(SoloGameStatus.PlayerWon)]
    [InlineData(SoloGameStatus.MachineWon)]
    [InlineData(SoloGameStatus.PlayerLost)]
    public async Task ExecuteAsync_WithDifferentFinishedStatuses_ShouldReturnCorrectStatus(SoloGameStatus status)
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.Status = status;
        game.GameFinishedAt = DateTime.UtcNow;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.Should().NotBeNull();
        result.Game.Status.Should().Be(status);
        result.Game.GameFinishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithVeryLongGame_ShouldHandleCorrectly()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.GameStartedAt = DateTime.UtcNow.AddHours(-1); // 1 hora de juego
        game.Status = SoloGameStatus.InProgress;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // Assert
        result.ElapsedTime.Should().BeGreaterOrEqualTo(3600); // Al menos 1 hora
        result.Game.MachinePosition.Should().Be(game.TotalQuestions); // La máquina debería haber terminado
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
            Questions = CreateTestQuestions(),
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

    /// <summary>
    /// Crea preguntas de prueba
    /// </summary>
    private static List<Question> CreateTestQuestions()
    {
        return new List<Question>
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
        };
    }

    #endregion
}