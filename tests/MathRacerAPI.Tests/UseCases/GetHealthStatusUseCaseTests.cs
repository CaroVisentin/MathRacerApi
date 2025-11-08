using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.UseCases;
using Xunit;

namespace MathRacerAPI.Tests.UseCases
{
    public class GetHealthStatusUseCaseTests
    {
        [Fact]
        public void Execute_ShouldReturnHealthStatusWithCorrectDefaults()
        {
            // Arrange
            var useCase = new GetHealthStatusUseCase();
            var before = DateTime.UtcNow;
            
            // Act
            var result = useCase.Execute();
            var after = DateTime.UtcNow;
            
            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Healthy");
            result.Version.Should().Be("1.0.0");
            result.Timestamp.Should().BeAfter(before.AddMilliseconds(-1));
            result.Timestamp.Should().BeBefore(after.AddMilliseconds(1));
            result.Details.Should().NotBeNull();
        }

        [Fact]
        public void Execute_ShouldAddUptimeDetail()
        {
            // Arrange
            var useCase = new GetHealthStatusUseCase();
            
            // Act
            var result = useCase.Execute();
            
            // Assert
            result.Details.Should().ContainKey("uptime");
            result.Details["uptime"].Should().BeOfType<DateTime>();
        }

        [Fact]
        public void Execute_ShouldAddMemoryDetail()
        {
            // Arrange
            var useCase = new GetHealthStatusUseCase();
            
            // Act
            var result = useCase.Execute();
            
            // Assert
            result.Details.Should().ContainKey("memory");
            result.Details["memory"].Should().BeOfType<long>();
            ((long)result.Details["memory"]).Should().BePositive();
        }

        [Fact]
        public void Execute_ShouldCreateNewInstanceEachTime()
        {
            // Arrange
            var useCase = new GetHealthStatusUseCase();
            
            // Act
            var result1 = useCase.Execute();
            System.Threading.Thread.Sleep(1); // Small delay
            var result2 = useCase.Execute();
            
            // Assert
            result1.Should().NotBeSameAs(result2);
            result1.Timestamp.Should().BeBefore(result2.Timestamp);
        }

        [Fact]
        public void Execute_ShouldIncludeExpectedDetailsKeys()
        {
            // Arrange
            var useCase = new GetHealthStatusUseCase();
            
            // Act
            var result = useCase.Execute();
            
            // Assert
            result.Details.Should().HaveCount(2);
            result.Details.Keys.Should().Contain("uptime");
            result.Details.Keys.Should().Contain("memory");
        }
    }
}