using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class ResultModelsTests
    {
        [Fact]
        public void NextQuestionResult_ShouldCreateWithDefaults()
        {
            // Act
            var result = new NextQuestionResult();
            
            // Assert
            result.Should().NotBeNull();
            result.Question.Should().BeNull();
            result.PenaltySecondsLeft.Should().BeNull();
        }

        [Fact]
        public void NextQuestionResult_ShouldAllowSettingProperties()
        {
            // Arrange
            var result = new NextQuestionResult();
            var question = new Question { Id = 1, Equation = "2+2=?" };
            
            // Act
            result.Question = question;
            result.PenaltySecondsLeft = 5.5;
            
            // Assert
            result.Question.Should().Be(question);
            result.PenaltySecondsLeft.Should().Be(5.5);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0.0)]
        [InlineData(10.5)]
        [InlineData(30.0)]
        public void NextQuestionResult_PenaltySecondsLeft_ShouldAcceptNullableDouble(double? penaltySeconds)
        {
            // Arrange
            var result = new NextQuestionResult();
            
            // Act
            result.PenaltySecondsLeft = penaltySeconds;
            
            // Assert
            result.PenaltySecondsLeft.Should().Be(penaltySeconds);
        }

        [Fact]
        public void SoloAnswerResult_ShouldCreateWithDefaults()
        {
            // Act
            var result = new SoloAnswerResult();
            
            // Assert
            result.Should().NotBeNull();
            result.Game.Should().BeNull(); // null! means it's expected to be assigned
            result.IsCorrect.Should().BeFalse();
            result.CorrectAnswer.Should().Be(0);
            result.PlayerAnswer.Should().Be(0);
            result.ShouldOpenWorldCompletionChest.Should().BeFalse();
            result.ProgressIncrement.Should().Be(1);
            result.CoinsEarned.Should().Be(0);
        }

        [Fact]
        public void SoloAnswerResult_ShouldAllowSettingProperties()
        {
            // Arrange
            var result = new SoloAnswerResult();
            var soloGame = new SoloGame { Id = 123, PlayerId = 456 };
            
            // Act
            result.Game = soloGame;
            result.IsCorrect = true;
            result.CorrectAnswer = 10;
            result.PlayerAnswer = 10;
            result.ShouldOpenWorldCompletionChest = true;
            result.ProgressIncrement = 2;
            result.CoinsEarned = 50;
            
            // Assert
            result.Game.Should().Be(soloGame);
            result.IsCorrect.Should().BeTrue();
            result.CorrectAnswer.Should().Be(10);
            result.PlayerAnswer.Should().Be(10);
            result.ShouldOpenWorldCompletionChest.Should().BeTrue();
            result.ProgressIncrement.Should().Be(2);
            result.CoinsEarned.Should().Be(50);
        }

        [Theory]
        [InlineData(true, 15, 15)]
        [InlineData(false, 10, 8)]
        [InlineData(false, 20, 25)]
        public void SoloAnswerResult_AnswerComparison_ShouldReflectCorrectness(bool isCorrect, int correctAnswer, int playerAnswer)
        {
            // Arrange
            var result = new SoloAnswerResult();
            
            // Act
            result.IsCorrect = isCorrect;
            result.CorrectAnswer = correctAnswer;
            result.PlayerAnswer = playerAnswer;
            
            // Assert
            result.IsCorrect.Should().Be(isCorrect);
            result.CorrectAnswer.Should().Be(correctAnswer);
            result.PlayerAnswer.Should().Be(playerAnswer);
            
            if (isCorrect)
            {
                result.CorrectAnswer.Should().Be(result.PlayerAnswer);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        public void SoloAnswerResult_ProgressIncrement_ShouldAcceptPositiveValues(int progressIncrement)
        {
            // Arrange
            var result = new SoloAnswerResult();
            
            // Act
            result.ProgressIncrement = progressIncrement;
            
            // Assert
            result.ProgressIncrement.Should().Be(progressIncrement);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void SoloAnswerResult_CoinsEarned_ShouldAcceptIntegerValues(int coinsEarned)
        {
            // Arrange
            var result = new SoloAnswerResult();
            
            // Act
            result.CoinsEarned = coinsEarned;
            
            // Assert
            result.CoinsEarned.Should().Be(coinsEarned);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SoloAnswerResult_ShouldOpenWorldCompletionChest_ShouldAcceptBooleanValues(bool shouldOpen)
        {
            // Arrange
            var result = new SoloAnswerResult();
            
            // Act
            result.ShouldOpenWorldCompletionChest = shouldOpen;
            
            // Assert
            result.ShouldOpenWorldCompletionChest.Should().Be(shouldOpen);
        }
    }
}