using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MathRacerAPI.Tests.UseCases
{
    /// <summary>
    /// Tests for GetPlayerGarageItemsUseCase
    /// </summary>
    public class GetPlayerGarageItemsUseCaseTests
    {
        private readonly Mock<IGarageRepository> _mockGarageRepository;
        private readonly GetPlayerGarageItemsUseCase _useCase;

        public GetPlayerGarageItemsUseCaseTests()
        {
            _mockGarageRepository = new Mock<IGarageRepository>();
            _useCase = new GetPlayerGarageItemsUseCase(_mockGarageRepository.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidPlayerIdAndItemType_ShouldReturnGarageItemsResponse()
        {
            // Arrange
            var playerId = 1;
            var itemType = "auto";
            var expectedResponse = CreateSampleGarageItemsResponse("Auto");

            _mockGarageRepository
                .Setup(x => x.GetPlayerItemsByTypeAsync(playerId, "Auto"))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _useCase.ExecuteAsync(playerId, itemType);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            result.ItemType.Should().Be("Auto");
            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidPlayerId_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidPlayerId = 0;
            var itemType = "car";

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(invalidPlayerId, itemType))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Player ID must be greater than 0*");
        }

        [Fact]
        public async Task ExecuteAsync_WithNullItemType_ShouldThrowArgumentException()
        {
            // Arrange
            var playerId = 1;
            string? itemType = null;

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(playerId, itemType!))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Item type cannot be null or empty*");
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyItemType_ShouldThrowArgumentException()
        {
            // Arrange
            var playerId = 1;
            var itemType = "";

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(playerId, itemType))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Item type cannot be null or empty*");
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidItemType_ShouldThrowArgumentException()
        {
            // Arrange
            var playerId = 1;
            var itemType = "invalid";

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(playerId, itemType))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid item type*");
        }

        [Theory]
        [InlineData("auto", "Auto")]
        [InlineData("Auto", "Auto")]
        [InlineData("AUTO", "Auto")]
        [InlineData("personaje", "Personaje")]
        [InlineData("PERSONAJE", "Personaje")]
        [InlineData("fondo", "Fondo")]
        [InlineData("FONDO", "Fondo")]
        public async Task ExecuteAsync_WithValidItemTypes_ShouldNormalizeAndProcess(string inputType, string expectedType)
        {
            // Arrange
            var playerId = 1;
            var expectedResponse = CreateSampleGarageItemsResponse(expectedType);

            _mockGarageRepository
                .Setup(x => x.GetPlayerItemsByTypeAsync(playerId, expectedType))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _useCase.ExecuteAsync(playerId, inputType);

            // Assert
            result.Should().NotBeNull();
            result.ItemType.Should().Be(expectedType);
            _mockGarageRepository.Verify(x => x.GetPlayerItemsByTypeAsync(playerId, expectedType), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithNegativePlayerId_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidPlayerId = -1;
            var itemType = "car";

            // Act & Assert
            await _useCase.Invoking(x => x.ExecuteAsync(invalidPlayerId, itemType))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Player ID must be greater than 0*");
        }

        private GarageItemsResponse CreateSampleGarageItemsResponse(string itemType)
        {
            return new GarageItemsResponse
            {
                ItemType = itemType,
                Items = new List<GarageItem>
                {
                    new GarageItem
                    {
                        Id = 1,
                        ProductId = 1,
                        Name = $"Test {itemType} 1",
                        Description = $"Description for test {itemType} 1",
                        Price = 100,
                        ProductType = itemType,
                        Rarity = "Common",
                        IsOwned = true,
                        IsActive = true
                    },
                    new GarageItem
                    {
                        Id = 2,
                        ProductId = 2,
                        Name = $"Test {itemType} 2",
                        Description = $"Description for test {itemType} 2",
                        Price = 200,
                        ProductType = itemType,
                        Rarity = "Rare",
                        IsOwned = false,
                        IsActive = false
                    }
                },
                ActiveItem = new GarageItem
                {
                    Id = 1,
                    ProductId = 1,
                    Name = $"Test {itemType} 1",
                    Description = $"Description for test {itemType} 1",
                    Price = 100,
                    ProductType = itemType,
                    Rarity = "Common",
                    IsOwned = true,
                    IsActive = true
                }
            };
        }
    }
}