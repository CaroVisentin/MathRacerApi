using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class LevelModelTests
    {
        [Fact]
        public void Level_ShouldCreateWithDefaults()
        {
            // Act
            var level = new Level();
            
            // Assert
            level.Should().NotBeNull();
            level.Id.Should().Be(0);
            level.WorldId.Should().Be(0);
            level.Number.Should().Be(0);
            level.TermsCount.Should().Be(0);
            level.VariablesCount.Should().Be(0);
            level.ResultType.Should().BeNull();
        }

        [Fact]
        public void Level_ShouldAllowSettingProperties()
        {
            // Arrange
            var level = new Level();
            
            // Act
            level.Id = 123;
            level.WorldId = 5;
            level.Number = 10;
            level.TermsCount = 3;
            level.VariablesCount = 2;
            level.ResultType = "MAYOR";
            
            // Assert
            level.Id.Should().Be(123);
            level.WorldId.Should().Be(5);
            level.Number.Should().Be(10);
            level.TermsCount.Should().Be(3);
            level.VariablesCount.Should().Be(2);
            level.ResultType.Should().Be("MAYOR");
        }

        [Theory]
        [InlineData("MAYOR")]
        [InlineData("MENOR")]
        [InlineData("IGUAL")]
        public void Level_ResultType_ShouldAcceptValidValues(string resultType)
        {
            // Arrange
            var level = new Level();
            
            // Act
            level.ResultType = resultType;
            
            // Assert
            level.ResultType.Should().Be(resultType);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(3, 2)]
        [InlineData(5, 4)]
        public void Level_TermsAndVariables_ShouldFollowLogicalConstraints(int termsCount, int variablesCount)
        {
            // Arrange
            var level = new Level();
            
            // Act
            level.TermsCount = termsCount;
            level.VariablesCount = variablesCount;
            
            // Assert
            level.TermsCount.Should().Be(termsCount);
            level.VariablesCount.Should().Be(variablesCount);
            // Typically variables should be less than or equal to terms
            level.VariablesCount.Should().BeLessOrEqualTo(level.TermsCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(25)]
        public void Level_Number_ShouldAcceptPositiveValues(int number)
        {
            // Arrange
            var level = new Level();
            
            // Act
            level.Number = number;
            
            // Assert
            level.Number.Should().Be(number);
            level.Number.Should().BePositive();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(2, 3)]
        public void Level_WorldId_ShouldRelateToWorld(int worldId, int levelNumber)
        {
            // Arrange
            var level = new Level();
            
            // Act
            level.WorldId = worldId;
            level.Number = levelNumber;
            
            // Assert
            level.WorldId.Should().Be(worldId);
            level.Number.Should().Be(levelNumber);
            // Level should belong to a world
            level.WorldId.Should().BePositive();
        }

        [Fact]
        public void Level_ShouldAllowMultipleInstancesWithDifferentProperties()
        {
            // Act
            var level1 = new Level { Id = 1, WorldId = 1, Number = 1, ResultType = "MAYOR" };
            var level2 = new Level { Id = 2, WorldId = 1, Number = 2, ResultType = "MENOR" };
            var level3 = new Level { Id = 3, WorldId = 2, Number = 1, ResultType = "IGUAL" };
            
            // Assert
            level1.Should().NotBeSameAs(level2);
            level1.Should().NotBeSameAs(level3);
            level2.Should().NotBeSameAs(level3);
            
            level1.Id.Should().NotBe(level2.Id);
            level1.Number.Should().Be(level3.Number); // Same number but different world
            level1.WorldId.Should().Be(level2.WorldId); // Same world but different level
        }
    }
}