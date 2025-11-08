using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class WorldModelTests
    {
        [Fact]
        public void World_ShouldCreateWithDefaults()
        {
            // Act
            var world = new World();
            
            // Assert
            world.Should().NotBeNull();
            world.Id.Should().Be(0);
            world.Name.Should().Be(string.Empty);
            world.OptionsCount.Should().Be(0);
            world.OptionRangeMin.Should().Be(0);
            world.OptionRangeMax.Should().Be(0);
            world.NumberRangeMin.Should().Be(0);
            world.NumberRangeMax.Should().Be(0);
            world.TimePerEquation.Should().Be(0);
            world.Difficulty.Should().Be(string.Empty);
            world.Levels.Should().NotBeNull();
            world.Levels.Should().BeEmpty();
        }

        [Fact]
        public void World_ShouldAllowSettingProperties()
        {
            // Arrange
            var world = new World();
            var levels = new List<Level> 
            { 
                new Level { Id = 1, Number = 1 },
                new Level { Id = 2, Number = 2 }
            };
            
            // Act
            world.Id = 123;
            world.Name = "Easy World";
            world.OptionsCount = 4;
            world.OptionRangeMin = 1;
            world.OptionRangeMax = 100;
            world.NumberRangeMin = 1;
            world.NumberRangeMax = 50;
            world.TimePerEquation = 30;
            world.Difficulty = "Easy";
            world.Levels = levels;
            
            // Assert
            world.Id.Should().Be(123);
            world.Name.Should().Be("Easy World");
            world.OptionsCount.Should().Be(4);
            world.OptionRangeMin.Should().Be(1);
            world.OptionRangeMax.Should().Be(100);
            world.NumberRangeMin.Should().Be(1);
            world.NumberRangeMax.Should().Be(50);
            world.TimePerEquation.Should().Be(30);
            world.Difficulty.Should().Be("Easy");
            world.Levels.Should().BeEquivalentTo(levels);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(5, 25)]
        [InlineData(10, 100)]
        public void World_RangeValues_ShouldAcceptValidRanges(int min, int max)
        {
            // Arrange
            var world = new World();
            
            // Act
            world.OptionRangeMin = min;
            world.OptionRangeMax = max;
            world.NumberRangeMin = min;
            world.NumberRangeMax = max;
            
            // Assert
            world.OptionRangeMin.Should().Be(min);
            world.OptionRangeMax.Should().Be(max);
            world.NumberRangeMin.Should().Be(min);
            world.NumberRangeMax.Should().Be(max);
            world.OptionRangeMax.Should().BeGreaterThanOrEqualTo(world.OptionRangeMin);
            world.NumberRangeMax.Should().BeGreaterThanOrEqualTo(world.NumberRangeMin);
        }

        [Theory]
        [InlineData("Easy")]
        [InlineData("Medium")]
        [InlineData("Hard")]
        [InlineData("Expert")]
        public void World_Difficulty_ShouldAcceptValidValues(string difficulty)
        {
            // Arrange
            var world = new World();
            
            // Act
            world.Difficulty = difficulty;
            
            // Assert
            world.Difficulty.Should().Be(difficulty);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        public void World_OptionsCount_ShouldAcceptValidValues(int optionsCount)
        {
            // Arrange
            var world = new World();
            
            // Act
            world.OptionsCount = optionsCount;
            
            // Assert
            world.OptionsCount.Should().Be(optionsCount);
            world.OptionsCount.Should().BePositive();
        }

        [Theory]
        [InlineData(10)]
        [InlineData(30)]
        [InlineData(60)]
        public void World_TimePerEquation_ShouldAcceptPositiveValues(int timePerEquation)
        {
            // Arrange
            var world = new World();
            
            // Act
            world.TimePerEquation = timePerEquation;
            
            // Assert
            world.TimePerEquation.Should().Be(timePerEquation);
            world.TimePerEquation.Should().BePositive();
        }

        [Fact]
        public void World_Levels_ShouldAllowManipulation()
        {
            // Arrange
            var world = new World();
            var level1 = new Level { Id = 1, Number = 1 };
            var level2 = new Level { Id = 2, Number = 2 };
            
            // Act
            world.Levels.Add(level1);
            world.Levels.Add(level2);
            
            // Assert
            world.Levels.Should().HaveCount(2);
            world.Levels.Should().Contain(level1);
            world.Levels.Should().Contain(level2);
            
            // Act - Remove level
            world.Levels.Remove(level1);
            
            // Assert
            world.Levels.Should().HaveCount(1);
            world.Levels.Should().NotContain(level1);
            world.Levels.Should().Contain(level2);
        }

        [Theory]
        [InlineData("World 1")]
        [InlineData("Arithmetic World")]
        [InlineData("Advanced Mathematics")]
        public void World_Name_ShouldAcceptStringValues(string name)
        {
            // Arrange
            var world = new World();
            
            // Act
            world.Name = name;
            
            // Assert
            world.Name.Should().Be(name);
        }
    }
}