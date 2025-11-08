using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class EnumTests
    {
        [Fact]
        public void GameStatus_ShouldHaveCorrectValues()
        {
            // Act & Assert
            GameStatus.WaitingForPlayers.Should().BeDefined();
            GameStatus.InProgress.Should().BeDefined();
            GameStatus.Finished.Should().BeDefined();
        }

        [Theory]
        [InlineData(GameStatus.WaitingForPlayers)]
        [InlineData(GameStatus.InProgress)]
        [InlineData(GameStatus.Finished)]
        public void GameStatus_AllValues_ShouldBeDefined(GameStatus status)
        {
            // Act & Assert
            status.Should().BeDefined();
        }

        [Fact]
        public void GameStatus_ShouldHaveExpectedEnumValues()
        {
            // Act & Assert
            ((int)GameStatus.WaitingForPlayers).Should().Be(0);
            ((int)GameStatus.InProgress).Should().Be(1);
            ((int)GameStatus.Finished).Should().Be(2);
        }

        [Fact]
        public void PowerUpType_ShouldHaveCorrectValues()
        {
            // Act & Assert
            PowerUpType.DoublePoints.Should().BeDefined();
            PowerUpType.ShuffleRival.Should().BeDefined();
            
            ((int)PowerUpType.DoublePoints).Should().Be(1);
            ((int)PowerUpType.ShuffleRival).Should().Be(2);
        }

        [Theory]
        [InlineData(PowerUpType.DoublePoints)]
        [InlineData(PowerUpType.ShuffleRival)]
        public void PowerUpType_AllValues_ShouldBeDefined(PowerUpType powerUpType)
        {
            // Act & Assert
            powerUpType.Should().BeDefined();
        }

        [Fact]
        public void GameStatus_ShouldSupportComparison()
        {
            // Act & Assert
            ((int)GameStatus.WaitingForPlayers).Should().BeLessThan((int)GameStatus.InProgress);
            ((int)GameStatus.InProgress).Should().BeLessThan((int)GameStatus.Finished);
            ((int)GameStatus.WaitingForPlayers).Should().BeLessThan((int)GameStatus.Finished);
        }

        [Fact]
        public void GameStatus_ShouldConvertToString()
        {
            // Act & Assert
            GameStatus.WaitingForPlayers.ToString().Should().Be("WaitingForPlayers");
            GameStatus.InProgress.ToString().Should().Be("InProgress");
            GameStatus.Finished.ToString().Should().Be("Finished");
        }

        [Fact]
        public void PowerUpType_ShouldConvertToString()
        {
            // Act & Assert
            PowerUpType.DoublePoints.ToString().Should().Be("DoublePoints");
            PowerUpType.ShuffleRival.ToString().Should().Be("ShuffleRival");
        }

        [Fact]
        public void Enums_ShouldSupportEquality()
        {
            // Act & Assert
            var status1 = GameStatus.InProgress;
            var status2 = GameStatus.InProgress;
            var status3 = GameStatus.Finished;
            
            status1.Should().Be(status2);
            status1.Should().NotBe(status3);
            
            var powerUp1 = PowerUpType.DoublePoints;
            var powerUp2 = PowerUpType.DoublePoints;
            var powerUp3 = PowerUpType.ShuffleRival;
            
            powerUp1.Should().Be(powerUp2);
            powerUp1.Should().NotBe(powerUp3);
        }
    }
}