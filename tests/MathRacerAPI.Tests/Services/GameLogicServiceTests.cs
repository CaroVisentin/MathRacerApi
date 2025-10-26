using Xunit;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Infrastructure.Services;
using MathRacerAPI.Domain.Services;

namespace MathRacerAPI.Tests.Services;

/// <summary>
/// Tests para el servicio de lógica de juego
/// </summary>
public class GameLogicServiceTests
{
    private readonly IGameLogicService _gameLogicService;

    public GameLogicServiceTests()
    {
        _gameLogicService = new GameLogicService();
    }

    #region CheckAndUpdateGameEndConditions Tests

    [Fact]
    public void CheckAndUpdateGameEndConditions_WhenPlayerReachesWinCondition_ShouldSetGameAsFinishedAndSetWinner()
    {
        // Arrange
        var game = CreateTestGame();
        var player1 = game.Players[0];
        var player2 = game.Players[1];
        
        player1.CorrectAnswers = 10; // Alcanza la condición de victoria
        player2.CorrectAnswers = 5;

        // Act
        var result = _gameLogicService.CheckAndUpdateGameEndConditions(game);

        // Assert
        Assert.True(result);
        Assert.Equal(GameStatus.Finished, game.Status);
        Assert.Equal(player1.Id, game.WinnerId);
        Assert.NotNull(player1.FinishedAt);
    }

    [Fact]
    public void CheckAndUpdateGameEndConditions_WhenNoPlayerReachesWinCondition_ShouldNotFinishGame()
    {
        // Arrange
        var game = CreateTestGame();
        var player1 = game.Players[0];
        var player2 = game.Players[1];
        
        player1.CorrectAnswers = 5;
        player2.CorrectAnswers = 3;

        // Act
        var result = _gameLogicService.CheckAndUpdateGameEndConditions(game);

        // Assert
        Assert.False(result);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.WinnerId);
    }

    [Fact]
    public void CheckAndUpdateGameEndConditions_WhenAllPlayersFinishAllQuestions_ShouldFinishGameWithHighestScoreWinner()
    {
        // Arrange
        var game = CreateTestGame();
        var player1 = game.Players[0];
        var player2 = game.Players[1];
        var player3 = game.Players[2]; // Incluir el tercer jugador
        
        // Todos terminaron todas las preguntas, pero player1 tiene más respuestas correctas
        player1.IndexAnswered = game.Questions.Count;
        player1.CorrectAnswers = 8;
        
        player2.IndexAnswered = game.Questions.Count;
        player2.CorrectAnswers = 6;
        
        player3.IndexAnswered = game.Questions.Count;
        player3.CorrectAnswers = 5;

        // Act
        var result = _gameLogicService.CheckAndUpdateGameEndConditions(game);

        // Assert
        Assert.True(result);
        Assert.Equal(GameStatus.Finished, game.Status);
        Assert.Equal(player1.Id, game.WinnerId);
        
        // El método debería haber marcado FinishedAt automáticamente
        Assert.NotNull(player1.FinishedAt);
        Assert.NotNull(player2.FinishedAt);
        Assert.NotNull(player3.FinishedAt);
    }

    [Fact]
    public void CheckAndUpdateGameEndConditions_WhenTieInScoreAllQuestionsFinished_ShouldUseTimeBreaker()
    {
        // Arrange
        var game = CreateTestGame();
        var player1 = game.Players[0];
        var player2 = game.Players[1];
        var player3 = game.Players[2]; // Incluir el tercer jugador
        
        // Los tres terminaron todas las preguntas, pero 1 y 2 empatan en puntaje
        player1.IndexAnswered = game.Questions.Count;
        player1.CorrectAnswers = 8;
        
        player2.IndexAnswered = game.Questions.Count;
        player2.CorrectAnswers = 8;
        
        player3.IndexAnswered = game.Questions.Count;
        player3.CorrectAnswers = 6; // Menos puntos

        // Act
        var result = _gameLogicService.CheckAndUpdateGameEndConditions(game);

        // Assert
        Assert.True(result);
        Assert.Equal(GameStatus.Finished, game.Status);
        
        // Debería haber un ganador entre player1 y player2 (desempate por tiempo)
        Assert.True(game.WinnerId.HasValue);
        Assert.True(game.WinnerId == player1.Id || game.WinnerId == player2.Id);
        
        // Todos deberían tener FinishedAt marcado
        Assert.NotNull(player1.FinishedAt);
        Assert.NotNull(player2.FinishedAt);
        Assert.NotNull(player3.FinishedAt);
    }

    #endregion

    #region UpdatePlayerPositions Tests

    [Fact]
    public void UpdatePlayerPositions_ShouldRankPlayersByCorrectAnswers()
    {
        // Arrange
        var game = CreateTestGame();
        var player1 = game.Players[0];
        var player2 = game.Players[1];
        var player3 = game.Players[2];
        
        player1.CorrectAnswers = 8;
        player2.CorrectAnswers = 10;
        player3.CorrectAnswers = 5;

        // Act
        _gameLogicService.UpdatePlayerPositions(game);

        // Assert
        Assert.Equal(2, player1.Position); // 8 respuestas correctas -> 2do lugar
        Assert.Equal(1, player2.Position); // 10 respuestas correctas -> 1er lugar
        Assert.Equal(3, player3.Position); // 5 respuestas correctas -> 3er lugar
    }

    [Fact]
    public void UpdatePlayerPositions_WhenTieInAnswers_ShouldUseTieBreaker()
    {
        // Arrange
        var game = CreateTestGame();
        var player1 = game.Players[0];
        var player2 = game.Players[1];
        var player3 = game.Players[2];
        
        player1.CorrectAnswers = 7;
        player2.CorrectAnswers = 7; // Empate con player1
        player3.CorrectAnswers = 5;
        
        // Player1 terminó antes que player2
        player1.FinishedAt = DateTime.UtcNow.AddMinutes(-2);
        player2.FinishedAt = DateTime.UtcNow.AddMinutes(-1);

        // Act
        _gameLogicService.UpdatePlayerPositions(game);

        // Assert
        Assert.Equal(1, player1.Position); // Ganó el desempate por tiempo
        Assert.Equal(2, player2.Position); // Perdió el desempate
        Assert.Equal(3, player3.Position); // Menos respuestas correctas
    }

    #endregion

    #region CanPlayerAnswer Tests

    [Fact]
    public void CanPlayerAnswer_WhenPlayerNotPenalized_ShouldReturnTrue()
    {
        // Arrange
        var player = new Player
        {
            Id = 1,
            Name = "TestPlayer",
            PenaltyUntil = null
        };

        // Act
        var result = _gameLogicService.CanPlayerAnswer(player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanPlayerAnswer_WhenPlayerPenalizedButExpired_ShouldReturnTrue()
    {
        // Arrange
        var player = new Player
        {
            Id = 1,
            Name = "TestPlayer",
            PenaltyUntil = DateTime.UtcNow.AddMinutes(-1) // Penalización expirada
        };

        // Act
        var result = _gameLogicService.CanPlayerAnswer(player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanPlayerAnswer_WhenPlayerCurrentlyPenalized_ShouldReturnFalse()
    {
        // Arrange
        var player = new Player
        {
            Id = 1,
            Name = "TestPlayer",
            PenaltyUntil = DateTime.UtcNow.AddMinutes(1) // Penalización activa
        };

        // Act
        var result = _gameLogicService.CanPlayerAnswer(player);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ApplyAnswerResult Tests

    [Fact]
    public void ApplyAnswerResult_WhenAnswerIsCorrect_ShouldIncreaseCorrectAnswers()
    {
        // Arrange
        var player = new Player
        {
            Id = 1,
            Name = "TestPlayer",
            CorrectAnswers = 5,
            IndexAnswered = 10
        };

        // Act
        _gameLogicService.ApplyAnswerResult(player, isCorrect: true);

        // Assert
        Assert.Equal(6, player.CorrectAnswers);
        Assert.Equal(11, player.IndexAnswered);
        Assert.Null(player.PenaltyUntil);
    }

    [Fact]
    public void ApplyAnswerResult_WhenAnswerIsIncorrect_ShouldApplyPenalty()
    {
        // Arrange
        var player = new Player
        {
            Id = 1,
            Name = "TestPlayer",
            CorrectAnswers = 5,
            IndexAnswered = 10
        };

        // Act
        _gameLogicService.ApplyAnswerResult(player, isCorrect: false);

        // Assert
        Assert.Equal(5, player.CorrectAnswers); // No aumenta
        Assert.Equal(11, player.IndexAnswered); // Sí aumenta el índice
        Assert.NotNull(player.PenaltyUntil);
        Assert.True(player.PenaltyUntil > DateTime.UtcNow); // Penalización en el futuro
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Crea un juego de prueba con configuración estándar
    /// </summary>
    private Game CreateTestGame()
    {
        var game = new Game
        {
            Id = 1,
            Status = GameStatus.InProgress,
            ConditionToWin = 10,
            MaxQuestions = 15,
            CreatedAt = DateTime.UtcNow,
            Questions = CreateTestQuestions(15)
        };

        // Agregar jugadores de prueba
        game.Players.Add(new Player
        {
            Id = 1,
            Name = "Player1",
            CorrectAnswers = 0,
            IndexAnswered = 0,
            Position = 0,
            IsReady = true
        });

        game.Players.Add(new Player
        {
            Id = 2,
            Name = "Player2",
            CorrectAnswers = 0,
            IndexAnswered = 0,
            Position = 0,
            IsReady = true
        });

        game.Players.Add(new Player
        {
            Id = 3,
            Name = "Player3",
            CorrectAnswers = 0,
            IndexAnswered = 0,
            Position = 0,
            IsReady = true
        });

        return game;
    }

    /// <summary>
    /// Crea preguntas de prueba
    /// </summary>
    private List<Question> CreateTestQuestions(int count)
    {
        var questions = new List<Question>();
        
        for (int i = 1; i <= count; i++)
        {
            questions.Add(new Question
            {
                Id = i,
                Equation = $"{i} + {i} = ?",
                CorrectAnswer = (i * 2),
                Options = new List<int> 
                { 
                    (i * 2), 
                    (i * 2 + 1), 
                    (i * 2 - 1), 
                    (i * 2 + 2) 
                }
            });
        }
        
        return questions;
    }

    #endregion
}