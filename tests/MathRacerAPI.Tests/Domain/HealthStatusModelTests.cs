using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class HealthStatusModelTests
    {
        [Fact]
        public void HealthStatus_ShouldCreateWithDefaults()
        {
            // Arrange
            var version = "2.0.0";
            var before = DateTime.UtcNow;
            
            // Act
            var healthStatus = new HealthStatus(version);
            var after = DateTime.UtcNow;
            
            // Assert
            healthStatus.Should().NotBeNull();
            healthStatus.Status.Should().Be("Healthy");
            healthStatus.Version.Should().Be(version);
            healthStatus.Timestamp.Should().BeAfter(before.AddMilliseconds(-1));
            healthStatus.Timestamp.Should().BeBefore(after.AddMilliseconds(1));
            healthStatus.Details.Should().NotBeNull();
            healthStatus.Details.Should().BeEmpty();
        }

        [Theory]
        [InlineData("1.0.0")]
        [InlineData("2.1.5")]
        [InlineData("3.0.0-beta")]
        public void HealthStatus_ShouldAcceptDifferentVersions(string version)
        {
            // Act
            var healthStatus = new HealthStatus(version);
            
            // Assert
            healthStatus.Version.Should().Be(version);
        }

        [Fact]
        public void AddDetail_ShouldAddKeyValuePairToDetails()
        {
            // Arrange
            var healthStatus = new HealthStatus("1.0.0");
            var key = "testKey";
            var value = "testValue";
            
            // Act
            healthStatus.AddDetail(key, value);
            
            // Assert
            healthStatus.Details.Should().ContainKey(key);
            healthStatus.Details[key].Should().Be(value);
        }

        [Fact]
        public void AddDetail_ShouldAcceptDifferentValueTypes()
        {
            // Arrange
            var healthStatus = new HealthStatus("1.0.0");
            
            // Act
            healthStatus.AddDetail("stringValue", "test");
            healthStatus.AddDetail("intValue", 123);
            healthStatus.AddDetail("dateValue", DateTime.UtcNow);
            healthStatus.AddDetail("boolValue", true);
            
            // Assert
            healthStatus.Details.Should().HaveCount(4);
            healthStatus.Details["stringValue"].Should().BeOfType<string>();
            healthStatus.Details["intValue"].Should().BeOfType<int>();
            healthStatus.Details["dateValue"].Should().BeOfType<DateTime>();
            healthStatus.Details["boolValue"].Should().BeOfType<bool>();
        }

        [Fact]
        public void AddDetail_ShouldOverwriteExistingKey()
        {
            // Arrange
            var healthStatus = new HealthStatus("1.0.0");
            var key = "testKey";
            
            // Act
            healthStatus.AddDetail(key, "value1");
            healthStatus.AddDetail(key, "value2");
            
            // Assert
            healthStatus.Details.Should().ContainKey(key);
            healthStatus.Details[key].Should().Be("value2");
            healthStatus.Details.Should().HaveCount(1);
        }

        [Fact]
        public void SetUnhealthy_ShouldChangeStatusAndAddReason()
        {
            // Arrange
            var healthStatus = new HealthStatus("1.0.0");
            var reason = "Database connection failed";
            
            // Act
            healthStatus.SetUnhealthy(reason);
            
            // Assert
            healthStatus.Status.Should().Be("Unhealthy");
            healthStatus.Details.Should().ContainKey("reason");
            healthStatus.Details["reason"].Should().Be(reason);
        }

        [Theory]
        [InlineData("Service unavailable")]
        [InlineData("Memory limit exceeded")]
        [InlineData("Network timeout")]
        public void SetUnhealthy_ShouldAcceptDifferentReasons(string reason)
        {
            // Arrange
            var healthStatus = new HealthStatus("1.0.0");
            
            // Act
            healthStatus.SetUnhealthy(reason);
            
            // Assert
            healthStatus.Status.Should().Be("Unhealthy");
            healthStatus.Details["reason"].Should().Be(reason);
        }

        [Fact]
        public void SetUnhealthy_ShouldOverwritePreviousReasonIfCalledMultipleTimes()
        {
            // Arrange
            var healthStatus = new HealthStatus("1.0.0");
            
            // Act
            healthStatus.SetUnhealthy("First reason");
            healthStatus.SetUnhealthy("Second reason");
            
            // Assert
            healthStatus.Status.Should().Be("Unhealthy");
            healthStatus.Details["reason"].Should().Be("Second reason");
        }

        [Fact]
        public void HealthStatus_ShouldMaintainDetailsAfterSetUnhealthy()
        {
            // Arrange
            var healthStatus = new HealthStatus("1.0.0");
            healthStatus.AddDetail("uptime", DateTime.UtcNow);
            healthStatus.AddDetail("memory", 1024L);
            
            // Act
            healthStatus.SetUnhealthy("Test failure");
            
            // Assert
            healthStatus.Details.Should().HaveCount(3); // uptime, memory, reason
            healthStatus.Details.Should().ContainKey("uptime");
            healthStatus.Details.Should().ContainKey("memory");
            healthStatus.Details.Should().ContainKey("reason");
        }
    }
}