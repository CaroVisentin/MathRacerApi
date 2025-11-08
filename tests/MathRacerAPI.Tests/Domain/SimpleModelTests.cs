using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Exceptions;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class SimpleModelTests
    {
        [Fact]
        public void Game_ShouldCreateWithDefaults()
        {
            // Act
            var game = new Game();
            
            // Assert
            game.Should().NotBeNull();
            game.Players.Should().NotBeNull();
            game.Questions.Should().NotBeNull();
            game.ActiveEffects.Should().NotBeNull();
            game.Status.Should().Be(GameStatus.WaitingForPlayers);
            game.MaxQuestions.Should().Be(40);
            game.ConditionToWin.Should().Be(10);
            game.ExpectedResult.Should().Be("MAYOR");
            game.PowerUpsEnabled.Should().BeTrue();
            game.MaxPowerUpsPerPlayer.Should().Be(3);
        }

        [Fact]
        public void Game_ShouldAllowSettingProperties()
        {
            // Arrange
            var game = new Game();
            
            // Act
            game.Id = 123;
            game.MaxQuestions = 50;
            game.ConditionToWin = 15;
            game.PowerUpsEnabled = false;
            
            // Assert
            game.Id.Should().Be(123);
            game.MaxQuestions.Should().Be(50);
            game.ConditionToWin.Should().Be(15);
            game.PowerUpsEnabled.Should().BeFalse();
        }

        [Fact]
        public void GameStatus_ShouldHaveValidValues()
        {
            // Act & Assert
            GameStatus.WaitingForPlayers.Should().BeDefined();
        }

        [Fact]
        public void BusinessException_ShouldCreateWithMessage()
        {
            // Arrange
            const string message = "Business rule violation";
            
            // Act
            var exception = new BusinessException(message);
            
            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.Should().BeOfType<BusinessException>();
        }

        [Fact]
        public void NotFoundException_ShouldCreateWithMessage()
        {
            // Arrange
            const string message = "Entity not found";
            
            // Act
            var exception = new NotFoundException(message);
            
            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.Should().BeOfType<NotFoundException>();
        }

        [Fact]
        public void ValidationException_ShouldCreateWithMessage()
        {
            // Arrange
            const string message = "Validation failed";
            
            // Act
            var exception = new ValidationException(message);
            
            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
            exception.Should().BeOfType<ValidationException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void Game_Id_ShouldAcceptDifferentValues(int id)
        {
            // Arrange
            var game = new Game();
            
            // Act
            game.Id = id;
            
            // Assert
            game.Id.Should().Be(id);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Game_PowerUpsEnabled_ShouldAcceptBooleanValues(bool enabled)
        {
            // Arrange
            var game = new Game();
            
            // Act
            game.PowerUpsEnabled = enabled;
            
            // Assert
            game.PowerUpsEnabled.Should().Be(enabled);
        }

        [Fact]
        public void Game_CreatedAt_ShouldBeSetAutomatically()
        {
            // Arrange
            var before = DateTime.UtcNow;
            
            // Act
            var game = new Game();
            var after = DateTime.UtcNow;
            
            // Assert
            game.CreatedAt.Should().BeAfter(before.AddMilliseconds(-1));
            game.CreatedAt.Should().BeBefore(after.AddMilliseconds(1));
        }
    }
}