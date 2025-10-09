using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.UseCases;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de envío de respuestas
/// </summary>
public class SubmitAnswerUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IGameLogicService> _gameLogicServiceMock;
    private readonly SubmitAnswerUseCase _submitAnswerUseCase;

    public SubmitAnswerUseCaseTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _gameLogicServiceMock = new Mock<IGameLogicService>();
        _submitAnswerUseCase = new SubmitAnswerUseCase(_gameRepositoryMock.Object, _gameLogicServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        const int gameId = 1;
        const int playerId = 1;
        const string answer = "10";

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((Game?)null);

        // Act
        var result = await _submitAnswerUseCase.ExecuteAsync(gameId, playerId, answer);

        // Assert
        result.Should().BeNull();
        _gameRepositoryMock.Verify(x => x.GetByIdAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameIsFinished_ShouldReturnGameWithoutProcessing()
    {
        // Arrange
        const int gameId = 1;
        const int playerId = 1;
        const string answer = "10";

        var game = CreateTestGame();
        game.Status = GameStatus.Finished;

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _submitAnswerUseCase.ExecuteAsync(gameId, playerId, answer);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(GameStatus.Finished);
        
        // No debería llamar a los servicios de lógica ni actualizar el repositorio
        _gameLogicServiceMock.Verify(x => x.CanPlayerAnswer(It.IsAny<Player>()), Times.Never);
        _gameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        const int gameId = 1;
        const int nonExistentPlayerId = 999;
        const string answer = "10";

        var game = CreateTestGame();
        
        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _submitAnswerUseCase.ExecuteAsync(gameId, nonExistentPlayerId, answer);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerIsPenalized_ShouldReturnGameWithoutProcessing()
    {
        // Arrange
        const int gameId = 1;
        const int playerId = 1;
        const string answer = "10";

        var game = CreateTestGame();
        var player = game.Players.First(p => p.Id == playerId);
        
        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _gameLogicServiceMock
            .Setup(x => x.CanPlayerAnswer(player))
            .Returns(false); // Jugador penalizado

        // Act
        var result = await _submitAnswerUseCase.ExecuteAsync(gameId, playerId, answer);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(game);
        
        // No debería procesar la respuesta
        _gameLogicServiceMock.Verify(x => x.ApplyAnswerResult(It.IsAny<Player>(), It.IsAny<bool>()), Times.Never);
        _gameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerFinishedAllQuestions_ShouldReturnGameWithoutProcessing()
    {
        // Arrange
        const int gameId = 1;
        const int playerId = 1;
        const string answer = "10";

        var game = CreateTestGame();
        var player = game.Players.First(p => p.Id == playerId);
        player.IndexAnswered = game.Questions.Count; // Ya terminó todas las preguntas
        
        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _gameLogicServiceMock
            .Setup(x => x.CanPlayerAnswer(player))
            .Returns(true);

        // Act
        var result = await _submitAnswerUseCase.ExecuteAsync(gameId, playerId, answer);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(game);
        
        // No debería procesar la respuesta
        _gameLogicServiceMock.Verify(x => x.ApplyAnswerResult(It.IsAny<Player>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAnswerIsCorrect_ShouldProcessCorrectAnswer()
    {
        // Arrange
        const int gameId = 1;
        const int playerId = 1;
        const string correctAnswer = "4"; // 2 + 2 = 4

        var game = CreateTestGame();
        var player = game.Players.First(p => p.Id == playerId);
        
        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _gameLogicServiceMock
            .Setup(x => x.CanPlayerAnswer(player))
            .Returns(true);

        // Act
        var result = await _submitAnswerUseCase.ExecuteAsync(gameId, playerId, correctAnswer);

        // Assert
        result.Should().NotBeNull();
        
        // Verificar que se llamaron los métodos correctos
        _gameLogicServiceMock.Verify(x => x.ApplyAnswerResult(player, true), Times.Once);
        _gameLogicServiceMock.Verify(x => x.UpdatePlayerPositions(game), Times.Once);
        _gameLogicServiceMock.Verify(x => x.CheckAndUpdateGameEndConditions(game), Times.Once);
        _gameRepositoryMock.Verify(x => x.UpdateAsync(game), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAnswerIsIncorrect_ShouldProcessIncorrectAnswer()
    {
        // Arrange
        const int gameId = 1;
        const int playerId = 1;
        const string incorrectAnswer = "5"; // 2 + 2 ≠ 5

        var game = CreateTestGame();
        var player = game.Players.First(p => p.Id == playerId);
        
        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _gameLogicServiceMock
            .Setup(x => x.CanPlayerAnswer(player))
            .Returns(true);

        // Act
        var result = await _submitAnswerUseCase.ExecuteAsync(gameId, playerId, incorrectAnswer);

        // Assert
        result.Should().NotBeNull();
        
        // Verificar que se llamaron los métodos correctos con respuesta incorrecta
        _gameLogicServiceMock.Verify(x => x.ApplyAnswerResult(player, false), Times.Once);
        _gameLogicServiceMock.Verify(x => x.UpdatePlayerPositions(game), Times.Once);
        _gameLogicServiceMock.Verify(x => x.CheckAndUpdateGameEndConditions(game), Times.Once);
        _gameRepositoryMock.Verify(x => x.UpdateAsync(game), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidAnswer_ShouldUpdateGameInRepository()
    {
        // Arrange
        const int gameId = 1;
        const int playerId = 1;
        const string answer = "4";

        var game = CreateTestGame();
        var player = game.Players.First(p => p.Id == playerId);
        
        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _gameLogicServiceMock
            .Setup(x => x.CanPlayerAnswer(player))
            .Returns(true);

        // Act
        await _submitAnswerUseCase.ExecuteAsync(gameId, playerId, answer);

        // Assert
        _gameRepositoryMock.Verify(x => x.UpdateAsync(game), Times.Once);
    }

    [Theory]
    [InlineData("4", true)]   // Respuesta correcta
    [InlineData("5", false)]  // Respuesta incorrecta
    [InlineData("3", false)]  // Respuesta incorrecta
    public async Task ExecuteAsync_WithDifferentAnswers_ShouldProcessCorrectly(string answer, bool expectedCorrect)
    {
        // Arrange
        const int gameId = 1;
        const int playerId = 1;

        var game = CreateTestGame();
        var player = game.Players.First(p => p.Id == playerId);
        
        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _gameLogicServiceMock
            .Setup(x => x.CanPlayerAnswer(player))
            .Returns(true);

        // Act
        var result = await _submitAnswerUseCase.ExecuteAsync(gameId, playerId, answer);

        // Assert
        result.Should().NotBeNull();
        _gameLogicServiceMock.Verify(x => x.ApplyAnswerResult(player, expectedCorrect), Times.Once);
    }

    #region Helper Methods

    /// <summary>
    /// Crea un juego de prueba con preguntas y jugadores
    /// </summary>
    private static Game CreateTestGame()
    {
        var questions = new List<Question>
        {
            new Question
            {
                Id = 1,
                Equation = "2 + 2 = ?",
                CorrectAnswer = "4",
                Options = new List<string> { "3", "4", "5", "6" }
            },
            new Question
            {
                Id = 2,
                Equation = "3 × 3 = ?",
                CorrectAnswer = "9",
                Options = new List<string> { "6", "9", "12", "15" }
            }
        };

        var game = new Game
        {
            Id = 1,
            Status = GameStatus.InProgress,
            ConditionToWin = 10,
            MaxQuestions = 2,
            CreatedAt = DateTime.UtcNow,
            Questions = questions
        };

        game.Players.Add(new Player
        {
            Id = 1,
            Name = "Player1",
            CorrectAnswers = 0,
            IndexAnswered = 0,
            Position = 1,
            IsReady = true
        });

        game.Players.Add(new Player
        {
            Id = 2,
            Name = "Player2",
            CorrectAnswers = 0,
            IndexAnswered = 0,
            Position = 1,
            IsReady = true
        });

        return game;
    }

    #endregion
}