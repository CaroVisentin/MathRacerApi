using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class EquationParamsModelTests
    {
        [Fact]
        public void EquationParams_ShouldCreateWithDefaults()
        {
            // Act
            var equationParams = new EquationParams();
            
            // Assert
            equationParams.Should().NotBeNull();
            equationParams.TermCount.Should().Be(0);
            equationParams.VariableCount.Should().Be(0);
            equationParams.Operations.Should().NotBeNull();
            equationParams.Operations.Should().BeEmpty();
            equationParams.ExpectedResult.Should().Be(string.Empty);
            equationParams.OptionsCount.Should().Be(0);
            equationParams.OptionRangeMin.Should().Be(0);
            equationParams.OptionRangeMax.Should().Be(0);
            equationParams.NumberRangeMin.Should().Be(0);
            equationParams.NumberRangeMax.Should().Be(0);
            equationParams.TimePerEquation.Should().Be(0);
        }

        [Fact]
        public void EquationParams_ShouldAllowSettingAllProperties()
        {
            // Arrange
            var equationParams = new EquationParams();
            var operations = new List<string> { "+", "-", "*", "/" };
            
            // Act
            equationParams.TermCount = 3;
            equationParams.VariableCount = 2;
            equationParams.Operations = operations;
            equationParams.ExpectedResult = "MAYOR";
            equationParams.OptionsCount = 4;
            equationParams.OptionRangeMin = 1;
            equationParams.OptionRangeMax = 100;
            equationParams.NumberRangeMin = 1;
            equationParams.NumberRangeMax = 50;
            equationParams.TimePerEquation = 30;
            
            // Assert
            equationParams.TermCount.Should().Be(3);
            equationParams.VariableCount.Should().Be(2);
            equationParams.Operations.Should().BeEquivalentTo(operations);
            equationParams.ExpectedResult.Should().Be("MAYOR");
            equationParams.OptionsCount.Should().Be(4);
            equationParams.OptionRangeMin.Should().Be(1);
            equationParams.OptionRangeMax.Should().Be(100);
            equationParams.NumberRangeMin.Should().Be(1);
            equationParams.NumberRangeMax.Should().Be(50);
            equationParams.TimePerEquation.Should().Be(30);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(5, 25)]
        [InlineData(10, 100)]
        public void EquationParams_RangeValues_ShouldAcceptValidRanges(int min, int max)
        {
            // Arrange
            var equationParams = new EquationParams();
            
            // Act
            equationParams.OptionRangeMin = min;
            equationParams.OptionRangeMax = max;
            equationParams.NumberRangeMin = min;
            equationParams.NumberRangeMax = max;
            
            // Assert
            equationParams.OptionRangeMin.Should().Be(min);
            equationParams.OptionRangeMax.Should().Be(max);
            equationParams.NumberRangeMin.Should().Be(min);
            equationParams.NumberRangeMax.Should().Be(max);
            equationParams.OptionRangeMax.Should().BeGreaterThanOrEqualTo(equationParams.OptionRangeMin);
            equationParams.NumberRangeMax.Should().BeGreaterThanOrEqualTo(equationParams.NumberRangeMin);
        }

        [Theory]
        [InlineData("MAYOR")]
        [InlineData("MENOR")]
        [InlineData("IGUAL")]
        public void EquationParams_ExpectedResult_ShouldAcceptValidValues(string expectedResult)
        {
            // Arrange
            var equationParams = new EquationParams();
            
            // Act
            equationParams.ExpectedResult = expectedResult;
            
            // Assert
            equationParams.ExpectedResult.Should().Be(expectedResult);
        }

        [Fact]
        public void EquationParams_Operations_ShouldAllowManipulation()
        {
            // Arrange
            var equationParams = new EquationParams();
            
            // Act
            equationParams.Operations.Add("+");
            equationParams.Operations.Add("-");
            equationParams.Operations.Add("*");
            
            // Assert
            equationParams.Operations.Should().HaveCount(3);
            equationParams.Operations.Should().Contain("+");
            equationParams.Operations.Should().Contain("-");
            equationParams.Operations.Should().Contain("*");
            
            // Act - Remove operation
            equationParams.Operations.Remove("+");
            
            // Assert
            equationParams.Operations.Should().HaveCount(2);
            equationParams.Operations.Should().NotContain("+");
        }

        [Theory]
        [InlineData(2, 1)]
        [InlineData(5, 3)]
        [InlineData(10, 8)]
        public void EquationParams_TermAndVariableCounts_ShouldBeLogical(int termCount, int variableCount)
        {
            // Arrange
            var equationParams = new EquationParams();
            
            // Act
            equationParams.TermCount = termCount;
            equationParams.VariableCount = variableCount;
            
            // Assert
            equationParams.TermCount.Should().Be(termCount);
            equationParams.VariableCount.Should().Be(variableCount);
            // Variable count should typically be less than or equal to term count
            equationParams.VariableCount.Should().BeLessOrEqualTo(equationParams.TermCount);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(30)]
        [InlineData(60)]
        public void EquationParams_TimePerEquation_ShouldAcceptPositiveValues(int timePerEquation)
        {
            // Arrange
            var equationParams = new EquationParams();
            
            // Act
            equationParams.TimePerEquation = timePerEquation;
            
            // Assert
            equationParams.TimePerEquation.Should().Be(timePerEquation);
            equationParams.TimePerEquation.Should().BePositive();
        }
    }
}