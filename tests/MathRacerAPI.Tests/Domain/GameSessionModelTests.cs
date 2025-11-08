using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class GameSessionModelTests
    {
        [Fact]
        public void GameSession_ShouldCreateWithDefaults()
        {
            // Act
            var gameSession = new GameSession();
            
            // Assert
            gameSession.Should().NotBeNull();
            gameSession.GameId.Should().Be(0);
            gameSession.Players.Should().NotBeNull();
            gameSession.Players.Should().BeEmpty();
            gameSession.Status.Should().Be(default(GameStatus));
            gameSession.CurrentQuestion.Should().BeNull();
            gameSession.WinnerId.Should().BeNull();
            gameSession.CreatedAt.Should().Be(default(DateTime));
            gameSession.QuestionCount.Should().Be(0);
            gameSession.ConditionToWin.Should().Be(0);
            gameSession.ExpectedResult.Should().BeNull();
            gameSession.ActiveEffects.Should().BeNull();
            gameSession.PowerUpsEnabled.Should().BeFalse();
        }

        [Fact]
        public void GameSession_ShouldAllowSettingProperties()
        {
            // Arrange
            var gameSession = new GameSession();
            var players = new List<Player> { new Player { Id = 1, Name = "Player1" } };
            var question = new Question { Id = 1, Equation = "2+2=?" };
            var activeEffects = new List<ActiveEffect>();
            var createdAt = DateTime.UtcNow;
            
            // Act
            gameSession.GameId = 123;
            gameSession.Players = players;
            gameSession.Status = GameStatus.WaitingForPlayers;
            gameSession.CurrentQuestion = question;
            gameSession.WinnerId = 1;
            gameSession.CreatedAt = createdAt;
            gameSession.QuestionCount = 10;
            gameSession.ConditionToWin = 5;
            gameSession.ExpectedResult = "MAYOR";
            gameSession.ActiveEffects = activeEffects;
            gameSession.PowerUpsEnabled = true;
            
            // Assert
            gameSession.GameId.Should().Be(123);
            gameSession.Players.Should().BeEquivalentTo(players);
            gameSession.Status.Should().Be(GameStatus.WaitingForPlayers);
            gameSession.CurrentQuestion.Should().Be(question);
            gameSession.WinnerId.Should().Be(1);
            gameSession.CreatedAt.Should().Be(createdAt);
            gameSession.QuestionCount.Should().Be(10);
            gameSession.ConditionToWin.Should().Be(5);
            gameSession.ExpectedResult.Should().Be("MAYOR");
            gameSession.ActiveEffects.Should().BeEquivalentTo(activeEffects);
            gameSession.PowerUpsEnabled.Should().BeTrue();
        }

        [Fact]
        public void GameSession_FromGame_ShouldCreateCorrectGameSession()
        {
            // Arrange
            var players = new List<Player> { new Player { Id = 1, Name = "Player1" } };
            var questions = new List<Question> 
            { 
                new Question { Id = 1, Equation = "1+1=?" },
                new Question { Id = 2, Equation = "2+2=?" }
            };
            var activeEffects = new List<ActiveEffect>();
            var createdAt = DateTime.UtcNow;
            
            var game = new Game
            {
                Id = 123,
                Players = players,
                Status = GameStatus.WaitingForPlayers,
                Questions = questions,
                WinnerId = null,
                CreatedAt = createdAt,
                ConditionToWin = 10,
                ExpectedResult = "MAYOR",
                ActiveEffects = activeEffects,
                PowerUpsEnabled = true
            };
            
            // Act
            var gameSession = GameSession.FromGame(game);
            
            // Assert
            gameSession.Should().NotBeNull();
            gameSession.GameId.Should().Be(game.Id);
            gameSession.Players.Should().BeEquivalentTo(game.Players);
            gameSession.Status.Should().Be(game.Status);
            gameSession.CurrentQuestion.Should().BeNull(); // Default parameter
            gameSession.WinnerId.Should().Be(game.WinnerId);
            gameSession.CreatedAt.Should().Be(game.CreatedAt);
            gameSession.QuestionCount.Should().Be(game.Questions.Count);
            gameSession.ConditionToWin.Should().Be(game.ConditionToWin);
            gameSession.ExpectedResult.Should().Be(game.ExpectedResult);
            gameSession.ActiveEffects.Should().BeEquivalentTo(game.ActiveEffects);
            gameSession.PowerUpsEnabled.Should().Be(game.PowerUpsEnabled);
        }

        [Fact]
        public void GameSession_FromGame_WithCurrentQuestion_ShouldIncludeCurrentQuestion()
        {
            // Arrange
            var game = new Game
            {
                Id = 123,
                Players = new List<Player>(),
                Questions = new List<Question> { new Question { Id = 1, Equation = "1+1=?" } }
            };
            var currentQuestion = new Question { Id = 2, Equation = "3+3=?" };
            
            // Act
            var gameSession = GameSession.FromGame(game, currentQuestion);
            
            // Assert
            gameSession.CurrentQuestion.Should().Be(currentQuestion);
            gameSession.CurrentQuestion.Should().NotBeNull();
            gameSession.CurrentQuestion!.Id.Should().Be(2);
            gameSession.CurrentQuestion.Equation.Should().Be("3+3=?");
        }

        [Fact]
        public void GameSession_FromGame_ShouldHandleNullableProperties()
        {
            // Arrange
            var game = new Game
            {
                Id = 456,
                WinnerId = 123,
                ExpectedResult = null, // Testing null value
                ActiveEffects = null   // Testing null value
            };
            
            // Act
            var gameSession = GameSession.FromGame(game);
            
            // Assert
            gameSession.GameId.Should().Be(456);
            gameSession.WinnerId.Should().Be(123);
            gameSession.ExpectedResult.Should().BeNull();
            gameSession.ActiveEffects.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(20)]
        public void GameSession_QuestionCount_ShouldReflectGameQuestionsCount(int questionCount)
        {
            // Arrange
            var game = new Game();
            for (int i = 0; i < questionCount; i++)
            {
                game.Questions.Add(new Question { Id = i + 1, Equation = $"{i}+1=?" });
            }
            
            // Act
            var gameSession = GameSession.FromGame(game);
            
            // Assert
            gameSession.QuestionCount.Should().Be(questionCount);
            gameSession.QuestionCount.Should().Be(game.Questions.Count);
        }
    }
}