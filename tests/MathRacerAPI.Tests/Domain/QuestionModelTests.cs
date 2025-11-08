using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class QuestionModelTests
    {
        [Fact]
        public void Question_ShouldCreateWithDefaults()
        {
            // Act
            var question = new Question();
            
            // Assert
            question.Should().NotBeNull();
            question.Equation.Should().Be(string.Empty);
            question.Options.Should().NotBeNull();
            question.Options.Should().BeEmpty();
            question.Id.Should().Be(0);
            question.CorrectAnswer.Should().Be(0);
        }

        [Fact]
        public void Question_ShouldAllowSettingProperties()
        {
            // Arrange
            var question = new Question();
            var options = new List<int> { 1, 2, 3, 4 };
            
            // Act
            question.Id = 123;
            question.Equation = "2 + 2 = ?";
            question.Options = options;
            question.CorrectAnswer = 4;
            
            // Assert
            question.Id.Should().Be(123);
            question.Equation.Should().Be("2 + 2 = ?");
            question.Options.Should().BeEquivalentTo(options);
            question.CorrectAnswer.Should().Be(4);
        }

        [Theory]
        [InlineData("1 + 1 = ?", 2)]
        [InlineData("5 * 3 = ?", 15)]
        [InlineData("10 - 4 = ?", 6)]
        public void Question_ShouldAcceptDifferentEquationsAndAnswers(string equation, int answer)
        {
            // Arrange
            var question = new Question();
            
            // Act
            question.Equation = equation;
            question.CorrectAnswer = answer;
            
            // Assert
            question.Equation.Should().Be(equation);
            question.CorrectAnswer.Should().Be(answer);
        }

        [Fact]
        public void Question_Options_ShouldAllowAddingItems()
        {
            // Arrange
            var question = new Question();
            
            // Act
            question.Options.Add(1);
            question.Options.Add(2);
            question.Options.Add(3);
            
            // Assert
            question.Options.Should().HaveCount(3);
            question.Options.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void Question_Options_ShouldAllowClearingItems()
        {
            // Arrange
            var question = new Question();
            question.Options.AddRange(new[] { 1, 2, 3, 4 });
            
            // Act
            question.Options.Clear();
            
            // Assert
            question.Options.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Simple equation")]
        public void Question_Equation_ShouldAcceptStringValues(string equation)
        {
            // Arrange
            var question = new Question();
            
            // Act
            question.Equation = equation;
            
            // Assert
            question.Equation.Should().Be(equation);
        }
    }
}