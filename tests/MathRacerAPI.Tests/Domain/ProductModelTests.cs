using FluentAssertions;
using MathRacerAPI.Domain.Models;
using Xunit;

namespace MathRacerAPI.Tests.Domain
{
    public class ProductModelTests
    {
        [Fact]
        public void Product_ShouldCreateWithDefaults()
        {
            // Act
            var product = new Product();
            
            // Assert
            product.Should().NotBeNull();
            product.Id.Should().Be(0);
            product.Name.Should().Be(string.Empty);
            product.Description.Should().Be(string.Empty);
            product.Price.Should().Be(0.0);
            product.ProductType.Should().Be(0);
            product.RarityId.Should().Be(0);
            product.RarityName.Should().Be(string.Empty);
            product.RarityColor.Should().Be(string.Empty);
            product.Players.Should().NotBeNull();
            product.Players.Should().BeEmpty();
        }

        [Fact]
        public void Product_ShouldAllowSettingProperties()
        {
            // Arrange
            var product = new Product();
            var players = new List<Player> { new Player { Id = 1, Name = "Player1" } };
            
            // Act
            product.Id = 100;
            product.Name = "Super Car";
            product.Description = "An amazing super car";
            product.Price = 299.99;
            product.ProductType = 1;
            product.RarityId = 3;
            product.RarityName = "Epic";
            product.RarityColor = "#9C27B0";
            product.Players = players;
            
            // Assert
            product.Id.Should().Be(100);
            product.Name.Should().Be("Super Car");
            product.Description.Should().Be("An amazing super car");
            product.Price.Should().Be(299.99);
            product.ProductType.Should().Be(1);
            product.RarityId.Should().Be(3);
            product.RarityName.Should().Be("Epic");
            product.RarityColor.Should().Be("#9C27B0");
            product.Players.Should().BeEquivalentTo(players);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(9.99)]
        [InlineData(199.95)]
        [InlineData(999.99)]
        public void Product_Price_ShouldAcceptDoubleValues(double price)
        {
            // Arrange
            var product = new Product();
            
            // Act
            product.Price = price;
            
            // Assert
            product.Price.Should().Be(price);
        }

        [Theory]
        [InlineData("Common", "#FFFFFF")]
        [InlineData("Rare", "#00FF00")]
        [InlineData("Epic", "#9C27B0")]
        [InlineData("Legendary", "#FFD700")]
        public void Product_RarityProperties_ShouldAcceptValues(string rarityName, string rarityColor)
        {
            // Arrange
            var product = new Product();
            
            // Act
            product.RarityName = rarityName;
            product.RarityColor = rarityColor;
            
            // Assert
            product.RarityName.Should().Be(rarityName);
            product.RarityColor.Should().Be(rarityColor);
        }

        [Fact]
        public void Product_Players_ShouldAllowManipulation()
        {
            // Arrange
            var product = new Product();
            var player1 = new Player { Id = 1, Name = "Player1" };
            var player2 = new Player { Id = 2, Name = "Player2" };
            
            // Act
            product.Players.Add(player1);
            product.Players.Add(player2);
            
            // Assert
            product.Players.Should().HaveCount(2);
            product.Players.Should().Contain(player1);
            product.Players.Should().Contain(player2);
            
            // Act - Remove player
            product.Players.Remove(player1);
            
            // Assert
            product.Players.Should().HaveCount(1);
            product.Players.Should().NotContain(player1);
            product.Players.Should().Contain(player2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void Product_ProductType_ShouldAcceptIntegerValues(int productType)
        {
            // Arrange
            var product = new Product();
            
            // Act
            product.ProductType = productType;
            
            // Assert
            product.ProductType.Should().Be(productType);
        }
    }
}