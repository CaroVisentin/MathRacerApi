using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class PowerUpModelTests
    {
        [Fact]
        public void PowerUp_ShouldCreateWithDefaults()
        {
            // Act
            var powerUp = new PowerUp();
            
            // Assert
            powerUp.Should().NotBeNull();
            powerUp.Id.Should().Be(0);
            powerUp.Type.Should().Be(default(PowerUpType));
            powerUp.Name.Should().Be(string.Empty);
            powerUp.Description.Should().Be(string.Empty);
        }

        [Fact]
        public void PowerUp_ShouldAllowSettingProperties()
        {
            // Arrange
            var powerUp = new PowerUp();
            
            // Act
            powerUp.Id = 123;
            powerUp.Type = PowerUpType.DoublePoints;
            powerUp.Name = "Double Points";
            powerUp.Description = "Next correct answer gives double points";
            
            // Assert
            powerUp.Id.Should().Be(123);
            powerUp.Type.Should().Be(PowerUpType.DoublePoints);
            powerUp.Name.Should().Be("Double Points");
            powerUp.Description.Should().Be("Next correct answer gives double points");
        }

        [Theory]
        [InlineData(PowerUpType.DoublePoints)]
        [InlineData(PowerUpType.ShuffleRival)]
        public void PowerUp_Type_ShouldAcceptValidEnumValues(PowerUpType type)
        {
            // Arrange
            var powerUp = new PowerUp();
            
            // Act
            powerUp.Type = type;
            
            // Assert
            powerUp.Type.Should().Be(type);
            powerUp.Type.Should().BeDefined();
        }

        [Fact]
        public void PowerUpType_ShouldHaveCorrectValues()
        {
            // Act & Assert
            PowerUpType.DoublePoints.Should().BeDefined();
            PowerUpType.ShuffleRival.Should().BeDefined();
            
            // Verify enum values
            ((int)PowerUpType.DoublePoints).Should().Be(1);
            ((int)PowerUpType.ShuffleRival).Should().Be(2);
        }

        [Theory]
        [InlineData("Freeze", "Freezes opponent for 3 seconds")]
        [InlineData("Speed Boost", "Increases answer speed")]
        [InlineData("", "")]
        public void PowerUp_NameAndDescription_ShouldAcceptStringValues(string name, string description)
        {
            // Arrange
            var powerUp = new PowerUp();
            
            // Act
            powerUp.Name = name;
            powerUp.Description = description;
            
            // Assert
            powerUp.Name.Should().Be(name);
            powerUp.Description.Should().Be(description);
        }

        [Fact]
        public void PowerUp_ShouldAllowCreatingMultipleInstances()
        {
            // Act
            var powerUp1 = new PowerUp { Id = 1, Type = PowerUpType.DoublePoints, Name = "Double" };
            var powerUp2 = new PowerUp { Id = 2, Type = PowerUpType.ShuffleRival, Name = "Shuffle" };
            
            // Assert
            powerUp1.Should().NotBeSameAs(powerUp2);
            powerUp1.Id.Should().NotBe(powerUp2.Id);
            powerUp1.Type.Should().NotBe(powerUp2.Type);
            powerUp1.Name.Should().NotBe(powerUp2.Name);
        }
    }
}