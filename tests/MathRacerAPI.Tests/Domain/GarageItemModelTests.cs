using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class GarageItemModelTests
    {
        [Fact]
        public void GarageItem_ShouldCreateWithDefaults()
        {
            // Act
            var item = new GarageItem();
            
            // Assert
            item.Should().NotBeNull();
            item.Id.Should().Be(0);
            item.ProductId.Should().Be(0);
            item.Name.Should().Be(string.Empty);
            item.Description.Should().Be(string.Empty);
            item.Price.Should().Be(0m);
            item.ProductType.Should().Be(string.Empty);
            item.Rarity.Should().Be(string.Empty);
            item.IsOwned.Should().BeFalse();
            item.IsActive.Should().BeFalse();
        }

        [Fact]
        public void GarageItem_ShouldAllowSettingProperties()
        {
            // Arrange
            var item = new GarageItem();
            
            // Act
            item.Id = 123;
            item.ProductId = 456;
            item.Name = "Racing Car";
            item.Description = "A fast racing car";
            item.Price = 99.99m;
            item.ProductType = "Vehicle";
            item.Rarity = "Legendary";
            item.IsOwned = true;
            item.IsActive = true;
            
            // Assert
            item.Id.Should().Be(123);
            item.ProductId.Should().Be(456);
            item.Name.Should().Be("Racing Car");
            item.Description.Should().Be("A fast racing car");
            item.Price.Should().Be(99.99m);
            item.ProductType.Should().Be("Vehicle");
            item.Rarity.Should().Be("Legendary");
            item.IsOwned.Should().BeTrue();
            item.IsActive.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10.5)]
        [InlineData(999.99)]
        public void GarageItem_Price_ShouldAcceptDecimalValues(decimal price)
        {
            // Arrange
            var item = new GarageItem();
            
            // Act
            item.Price = price;
            
            // Assert
            item.Price.Should().Be(price);
        }

        [Theory]
        [InlineData("Common")]
        [InlineData("Rare")]
        [InlineData("Epic")]
        [InlineData("Legendary")]
        public void GarageItem_Rarity_ShouldAcceptDifferentValues(string rarity)
        {
            // Arrange
            var item = new GarageItem();
            
            // Act
            item.Rarity = rarity;
            
            // Assert
            item.Rarity.Should().Be(rarity);
        }

        [Theory]
        [InlineData("Vehicle")]
        [InlineData("Weapon")]
        [InlineData("Cosmetic")]
        public void GarageItem_ProductType_ShouldAcceptDifferentTypes(string productType)
        {
            // Arrange
            var item = new GarageItem();
            
            // Act
            item.ProductType = productType;
            
            // Assert
            item.ProductType.Should().Be(productType);
        }

        [Fact]
        public void GarageItem_BooleanProperties_ShouldToggle()
        {
            // Arrange
            var item = new GarageItem();
            
            // Act & Assert - Initial state
            item.IsOwned.Should().BeFalse();
            item.IsActive.Should().BeFalse();
            
            // Act & Assert - Toggle to true
            item.IsOwned = true;
            item.IsActive = true;
            item.IsOwned.Should().BeTrue();
            item.IsActive.Should().BeTrue();
            
            // Act & Assert - Toggle back to false
            item.IsOwned = false;
            item.IsActive = false;
            item.IsOwned.Should().BeFalse();
            item.IsActive.Should().BeFalse();
        }
    }
}