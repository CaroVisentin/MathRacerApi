using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MathRacerAPI.Tests.UseCases
{
    /// <summary>
    /// Tests for ActivatePlayerItemUseCase
    /// </summary>
    public class ActivatePlayerItemUseCaseTests
    {
        private readonly Mock<IGarageRepository> _mockGarageRepository;
        private readonly ActivatePlayerItemUseCase _useCase;

        public ActivatePlayerItemUseCaseTests()
        {
            _mockGarageRepository = new Mock<IGarageRepository>();
            _useCase = new ActivatePlayerItemUseCase(_mockGarageRepository.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidRequest_ShouldReturnTrue()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 1,
                ProductId = 1,
                ProductType = "car"
            };

            _mockGarageRepository
                .Setup(x => x.ActivatePlayerItemAsync(1, 1, "Car"))
                .ReturnsAsync(true);

            // Act
            var result = await _useCase.ExecuteAsync(request);

            // Assert
            result.Should().BeTrue();
            _mockGarageRepository.Verify(x => x.ActivatePlayerItemAsync(1, 1, "Car"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithPlayerNotOwningItem_ShouldReturnFalse()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 1,
                ProductId = 1,
                ProductType = "car"
            };

            _mockGarageRepository
                .Setup(x => x.ActivatePlayerItemAsync(1, 1, "Car"))
                .ReturnsAsync(false);

            // Act
            var result = await _useCase.ExecuteAsync(request);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteAsync_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Arrange
            ActivateItemRequest? request = null;

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(request!))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidPlayerId_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 0,
                ProductId = 1,
                ProductType = "car"
            };

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(request))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Player ID must be greater than 0*");
        }

        [Fact]
        public async Task ExecuteAsync_WithNegativePlayerId_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = -1,
                ProductId = 1,
                ProductType = "car"
            };

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(request))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Player ID must be greater than 0*");
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidProductId_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 1,
                ProductId = 0,
                ProductType = "car"
            };

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(request))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Product ID must be greater than 0*");
        }

        [Fact]
        public async Task ExecuteAsync_WithNegativeProductId_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 1,
                ProductId = -1,
                ProductType = "car"
            };

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(request))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Product ID must be greater than 0*");
        }

        [Fact]
        public async Task ExecuteAsync_WithNullProductType_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 1,
                ProductId = 1,
                ProductType = null!
            };

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(request))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Product type cannot be null or empty*");
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyProductType_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 1,
                ProductId = 1,
                ProductType = ""
            };

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(request))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Product type cannot be null or empty*");
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidProductType_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 1,
                ProductId = 1,
                ProductType = "invalid"
            };

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(request))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid product type*");
        }

        [Theory]
        [InlineData("car", "Car")]
        [InlineData("Car", "Car")]
        [InlineData("CAR", "Car")]
        [InlineData("character", "Character")]
        [InlineData("CHARACTER", "Character")]
        [InlineData("background", "Background")]
        [InlineData("BACKGROUND", "Background")]
        public async Task ExecuteAsync_WithValidProductTypes_ShouldNormalizeAndProcess(string inputType, string expectedType)
        {
            // Arrange
            var request = new ActivateItemRequest
            {
                PlayerId = 1,
                ProductId = 1,
                ProductType = inputType
            };

            _mockGarageRepository
                .Setup(x => x.ActivatePlayerItemAsync(1, 1, expectedType))
                .ReturnsAsync(true);

            // Act
            var result = await _useCase.ExecuteAsync(request);

            // Assert
            result.Should().BeTrue();
            _mockGarageRepository.Verify(x => x.ActivatePlayerItemAsync(1, 1, expectedType), Times.Once);
        }
    }
}