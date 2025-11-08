using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class PlayerModelTests
    {
        [Fact]
        public void Player_ShouldCreateWithDefaults()
        {
            // Act
            var player = new Player();
            
            // Assert
            player.Should().NotBeNull();
            player.Name.Should().Be(string.Empty);
            player.ConnectionId.Should().Be(string.Empty);
            player.Id.Should().Be(0);
            player.LastLevelId.Should().Be(0);
            player.CorrectAnswers.Should().Be(0);
            player.IndexAnswered.Should().Be(0);
            player.Position.Should().Be(0);
            player.IsReady.Should().BeFalse();
            player.PenaltyUntil.Should().BeNull();
        }

        [Fact]
        public void Player_ShouldAllowSettingProperties()
        {
            // Arrange
            var player = new Player();
            var penaltyTime = DateTime.UtcNow.AddMinutes(5);
            
            // Act
            player.Id = 123;
            player.Name = "TestPlayer";
            player.ConnectionId = "conn123";
            player.LastLevelId = 5;
            player.CorrectAnswers = 10;
            player.IndexAnswered = 8;
            player.Position = 2;
            player.IsReady = true;
            player.PenaltyUntil = penaltyTime;
            
            // Assert
            player.Id.Should().Be(123);
            player.Name.Should().Be("TestPlayer");
            player.ConnectionId.Should().Be("conn123");
            player.LastLevelId.Should().Be(5);
            player.CorrectAnswers.Should().Be(10);
            player.IndexAnswered.Should().Be(8);
            player.Position.Should().Be(2);
            player.IsReady.Should().BeTrue();
            player.PenaltyUntil.Should().Be(penaltyTime);
        }

        [Theory]
        [InlineData("Player1")]
        [InlineData("TestUser123")]
        [InlineData("")]
        public void Player_Name_ShouldAcceptStringValues(string name)
        {
            // Arrange
            var player = new Player();
            
            // Act
            player.Name = name;
            
            // Assert
            player.Name.Should().Be(name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(100)]
        public void Player_CorrectAnswers_ShouldAcceptIntegerValues(int answers)
        {
            // Arrange
            var player = new Player();
            
            // Act
            player.CorrectAnswers = answers;
            
            // Assert
            player.CorrectAnswers.Should().Be(answers);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Player_IsReady_ShouldAcceptBooleanValues(bool isReady)
        {
            // Arrange
            var player = new Player();
            
            // Act
            player.IsReady = isReady;
            
            // Assert
            player.IsReady.Should().Be(isReady);
        }

        [Fact]
        public void Player_PenaltyUntil_ShouldAcceptNullableDateTime()
        {
            // Arrange
            var player = new Player();
            var penaltyTime = DateTime.UtcNow.AddHours(1);
            
            // Act & Assert - Setting to specific time
            player.PenaltyUntil = penaltyTime;
            player.PenaltyUntil.Should().Be(penaltyTime);
            
            // Act & Assert - Setting to null
            player.PenaltyUntil = null;
            player.PenaltyUntil.Should().BeNull();
        }
    }
}