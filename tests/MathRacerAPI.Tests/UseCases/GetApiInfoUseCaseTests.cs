using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.UseCases;
using Xunit;

namespace MathRacerAPI.Tests.UseCases
{
    public class GetApiInfoUseCaseTests
    {
        [Fact]
        public void Execute_ShouldReturnApiInfoWithCorrectProperties()
        {
            // Arrange
            var useCase = new GetApiInfoUseCase();
            var environment = "Development";
            
            // Act
            var result = useCase.Execute(environment);
            
            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("MathRacer API");
            result.Version.Should().Be("1.0.0");
            result.Description.Should().Be("API para competencias matemáticas en tiempo real");
            result.Environment.Should().Be(environment);
            result.Status.Should().Be("Running");
            result.Endpoints.Should().NotBeNull();
        }

        [Theory]
        [InlineData("Development")]
        [InlineData("Production")]
        [InlineData("Testing")]
        public void Execute_ShouldAcceptDifferentEnvironments(string environment)
        {
            // Arrange
            var useCase = new GetApiInfoUseCase();
            
            // Act
            var result = useCase.Execute(environment);
            
            // Assert
            result.Environment.Should().Be(environment);
        }

        [Fact]
        public void Execute_ShouldSetTimestampToCurrentTime()
        {
            // Arrange
            var useCase = new GetApiInfoUseCase();
            var before = DateTime.UtcNow;
            
            // Act
            var result = useCase.Execute("Test");
            var after = DateTime.UtcNow;
            
            // Assert
            result.Timestamp.Should().BeAfter(before.AddMilliseconds(-1));
            result.Timestamp.Should().BeBefore(after.AddMilliseconds(1));
        }

        [Fact]
        public void Execute_ShouldCreateEndpointsWithCorrectValues()
        {
            // Arrange
            var useCase = new GetApiInfoUseCase();
            
            // Act
            var result = useCase.Execute("Test");
            
            // Assert
            result.Endpoints.Health.Should().Be("/health");
            result.Endpoints.Swagger.Should().Be("/swagger");
            result.Endpoints.ApiInfo.Should().Be("/api/info");
        }

        [Fact]
        public void Execute_ShouldCreateNewInstanceEachTime()
        {
            // Arrange
            var useCase = new GetApiInfoUseCase();
            
            // Act
            var result1 = useCase.Execute("Test1");
            var result2 = useCase.Execute("Test2");
            
            // Assert
            result1.Should().NotBeSameAs(result2);
            result1.Environment.Should().NotBe(result2.Environment);
            result1.Timestamp.Should().BeBefore(result2.Timestamp);
        }
    }
}