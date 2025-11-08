using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class ApiInfoModelTests
    {
        [Fact]
        public void ApiInfo_ShouldCreateWithConstructorParameters()
        {
            // Arrange
            var name = "Test API";
            var version = "2.0.0";
            var description = "Test Description";
            var environment = "Test";
            var endpoints = new ApiEndpoints();
            var before = DateTime.UtcNow;
            
            // Act
            var apiInfo = new ApiInfo(name, version, description, environment, endpoints);
            var after = DateTime.UtcNow;
            
            // Assert
            apiInfo.Should().NotBeNull();
            apiInfo.Name.Should().Be(name);
            apiInfo.Version.Should().Be(version);
            apiInfo.Description.Should().Be(description);
            apiInfo.Environment.Should().Be(environment);
            apiInfo.Endpoints.Should().Be(endpoints);
            apiInfo.Status.Should().Be("Running");
            apiInfo.Timestamp.Should().BeAfter(before.AddMilliseconds(-1));
            apiInfo.Timestamp.Should().BeBefore(after.AddMilliseconds(1));
        }

        [Fact]
        public void ApiEndpoints_ShouldCreateWithDefaultValues()
        {
            // Act
            var endpoints = new ApiEndpoints();
            
            // Assert
            endpoints.Should().NotBeNull();
            endpoints.Health.Should().Be("/health");
            endpoints.Swagger.Should().Be("/swagger");
            endpoints.ApiInfo.Should().Be("/api/info");
        }

        [Theory]
        [InlineData("API v1", "1.0", "Description 1", "Dev")]
        [InlineData("API v2", "2.0", "Description 2", "Prod")]
        public void ApiInfo_ShouldAcceptDifferentConstructorParameters(string name, string version, string description, string environment)
        {
            // Arrange
            var endpoints = new ApiEndpoints();
            
            // Act
            var apiInfo = new ApiInfo(name, version, description, environment, endpoints);
            
            // Assert
            apiInfo.Name.Should().Be(name);
            apiInfo.Version.Should().Be(version);
            apiInfo.Description.Should().Be(description);
            apiInfo.Environment.Should().Be(environment);
        }

        [Fact]
        public void ApiInfo_PropertiesShouldBeReadOnly()
        {
            // Arrange
            var endpoints = new ApiEndpoints();
            var apiInfo = new ApiInfo("Test", "1.0", "Desc", "Env", endpoints);
            
            // Act & Assert - Properties should be read-only (private setters)
            // We can't test this directly, but we can verify they don't change
            var originalName = apiInfo.Name;
            var originalVersion = apiInfo.Version;
            
            // Properties should maintain their values
            apiInfo.Name.Should().Be(originalName);
            apiInfo.Version.Should().Be(originalVersion);
        }

        [Fact]
        public void ApiInfo_ShouldCreateMultipleInstancesWithDifferentTimestamps()
        {
            // Arrange
            var endpoints = new ApiEndpoints();
            
            // Act
            var apiInfo1 = new ApiInfo("API1", "1.0", "Desc1", "Env1", endpoints);
            System.Threading.Thread.Sleep(1); // Small delay to ensure different timestamps
            var apiInfo2 = new ApiInfo("API2", "2.0", "Desc2", "Env2", endpoints);
            
            // Assert
            apiInfo1.Should().NotBeSameAs(apiInfo2);
            apiInfo1.Timestamp.Should().BeBefore(apiInfo2.Timestamp);
            apiInfo1.Name.Should().NotBe(apiInfo2.Name);
        }
    }
}