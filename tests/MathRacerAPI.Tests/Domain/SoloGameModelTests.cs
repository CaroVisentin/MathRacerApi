using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class SoloGameModelTests
    {
        [Fact]
        public void SoloGame_ShouldCreateWithDefaults()
        {
            // Act
            var soloGame = new SoloGame();
            
            // Assert
            soloGame.Should().NotBeNull();
            soloGame.PlayerUid.Should().Be(string.Empty);
            soloGame.PlayerName.Should().Be(string.Empty);
            soloGame.ResultType.Should().Be(string.Empty);
            soloGame.Questions.Should().NotBeNull();
            soloGame.PlayerPosition.Should().Be(0);
            soloGame.LivesRemaining.Should().Be(3);
            soloGame.CorrectAnswers.Should().Be(0);
            soloGame.CurrentQuestionIndex.Should().Be(0);
            soloGame.MachinePosition.Should().Be(0);
            soloGame.TotalQuestions.Should().Be(10);
        }

        [Fact]
        public void SoloGame_ShouldAllowSettingBasicProperties()
        {
            // Arrange
            var soloGame = new SoloGame();
            
            // Act
            soloGame.Id = 123;
            soloGame.PlayerId = 456;
            soloGame.PlayerUid = "uid123";
            soloGame.PlayerName = "TestPlayer";
            soloGame.LevelId = 5;
            soloGame.WorldId = 2;
            soloGame.ResultType = "WIN";
            
            // Assert
            soloGame.Id.Should().Be(123);
            soloGame.PlayerId.Should().Be(456);
            soloGame.PlayerUid.Should().Be("uid123");
            soloGame.PlayerName.Should().Be("TestPlayer");
            soloGame.LevelId.Should().Be(5);
            soloGame.WorldId.Should().Be(2);
            soloGame.ResultType.Should().Be("WIN");
        }

        [Fact]
        public void SoloGame_ShouldAllowSettingGameProgressProperties()
        {
            // Arrange
            var soloGame = new SoloGame();
            
            // Act
            soloGame.PlayerPosition = 15;
            soloGame.LivesRemaining = 1;
            soloGame.CorrectAnswers = 8;
            soloGame.CurrentQuestionIndex = 7;
            soloGame.MachinePosition = 12;
            soloGame.TotalQuestions = 20;
            
            // Assert
            soloGame.PlayerPosition.Should().Be(15);
            soloGame.LivesRemaining.Should().Be(1);
            soloGame.CorrectAnswers.Should().Be(8);
            soloGame.CurrentQuestionIndex.Should().Be(7);
            soloGame.MachinePosition.Should().Be(12);
            soloGame.TotalQuestions.Should().Be(20);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void SoloGame_LivesRemaining_ShouldAcceptValidValues(int lives)
        {
            // Arrange
            var soloGame = new SoloGame();
            
            // Act
            soloGame.LivesRemaining = lives;
            
            // Assert
            soloGame.LivesRemaining.Should().Be(lives);
        }

        [Theory]
        [InlineData("WIN")]
        [InlineData("LOSE")]
        [InlineData("IN_PROGRESS")]
        public void SoloGame_ResultType_ShouldAcceptStringValues(string resultType)
        {
            // Arrange
            var soloGame = new SoloGame();
            
            // Act
            soloGame.ResultType = resultType;
            
            // Assert
            soloGame.ResultType.Should().Be(resultType);
        }

        [Fact]
        public void SoloGame_Questions_ShouldAllowAddingItems()
        {
            // Arrange
            var soloGame = new SoloGame();
            var question1 = new Question { Id = 1, Equation = "1+1=?" };
            var question2 = new Question { Id = 2, Equation = "2+2=?" };
            
            // Act
            soloGame.Questions.Add(question1);
            soloGame.Questions.Add(question2);
            
            // Assert
            soloGame.Questions.Should().HaveCount(2);
            soloGame.Questions.Should().Contain(question1);
            soloGame.Questions.Should().Contain(question2);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(5, 10)]
        [InlineData(15, 8)]
        public void SoloGame_Positions_ShouldAllowComparison(int playerPos, int machinePos)
        {
            // Arrange
            var soloGame = new SoloGame();
            
            // Act
            soloGame.PlayerPosition = playerPos;
            soloGame.MachinePosition = machinePos;
            
            // Assert
            soloGame.PlayerPosition.Should().Be(playerPos);
            soloGame.MachinePosition.Should().Be(machinePos);
            
            if (playerPos > machinePos)
            {
                soloGame.PlayerPosition.Should().BeGreaterThan(soloGame.MachinePosition);
            }
            else if (playerPos < machinePos)
            {
                soloGame.PlayerPosition.Should().BeLessThan(soloGame.MachinePosition);
            }
            else
            {
                soloGame.PlayerPosition.Should().Be(soloGame.MachinePosition);
            }
        }
    }
}