using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para los casos de uso de la tienda
/// </summary>
public class StoreUseCasesBasicTests
{
    #region PurchaseStoreItemUseCase Basic Tests

    [Fact]
    public async Task PurchaseStoreItemUseCase_WithInvalidPlayer_ShouldThrowNotFoundException()
    {
        // Arrange
        var storeRepositoryMock = new Mock<IStoreRepository>();
        var playerRepositoryMock = new Mock<IPlayerRepository>();
        
        const int invalidPlayerId = 999;
        const int productId = 10;

        playerRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidPlayerId))
            .ReturnsAsync((PlayerProfile?)null);

        var useCase = new PurchaseStoreItemUseCase(storeRepositoryMock.Object, playerRepositoryMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => useCase.ExecuteAsync(invalidPlayerId, productId));

        exception.Message.Should().Be("Jugador no encontrado");
    }

    [Fact]
    public async Task PurchaseStoreItemUseCase_WithInvalidProduct_ShouldThrowBusinessException()
    {
        // Arrange
        var storeRepositoryMock = new Mock<IStoreRepository>();
        var playerRepositoryMock = new Mock<IPlayerRepository>();
        
        const int playerId = 1;
        const int invalidProductId = 999;

        var player = new PlayerProfile
        {
            Id = playerId,
            Name = "TestPlayer",
            Email = "test@test.com",
            Uid = "uid_test",
            Coins = 1000,
            LastLevelId = 1,
            Points = 100
        };

        playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        storeRepositoryMock
            .Setup(x => x.GetProductByIdAsync(invalidProductId, playerId))
            .ReturnsAsync((StoreItem?)null);

        var useCase = new PurchaseStoreItemUseCase(storeRepositoryMock.Object, playerRepositoryMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => useCase.ExecuteAsync(playerId, invalidProductId));

        exception.Message.Should().Be("Producto no encontrado");
    }

    [Fact]
    public async Task PurchaseStoreItemUseCase_WithAlreadyOwnedProduct_ShouldThrowConflictException()
    {
        // Arrange
        var storeRepositoryMock = new Mock<IStoreRepository>();
        var playerRepositoryMock = new Mock<IPlayerRepository>();
        
        const int playerId = 1;
        const int productId = 10;
        const int playerCoins = 1000;

        var player = new PlayerProfile
        {
            Id = playerId,
            Name = "TestPlayer",
            Email = "test@test.com", 
            Uid = "uid_test",
            Coins = playerCoins,
            LastLevelId = 1,
            Points = 100
        };

        var ownedProduct = new StoreItem
        {
            Id = productId,
            Name = "Producto Test",
            Description = "Descripción del producto test",
            Price = 500,
            ImageUrl = "",
            ProductTypeId = 1,
            ProductTypeName = "Auto",
            Rarity = "Común",
            IsOwned = true, // Already owned
            Currency = "Coins"
        };

        playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        storeRepositoryMock
            .Setup(x => x.GetProductByIdAsync(productId, playerId))
            .ReturnsAsync(ownedProduct);

        var useCase = new PurchaseStoreItemUseCase(storeRepositoryMock.Object, playerRepositoryMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => useCase.ExecuteAsync(playerId, productId));

        exception.Message.Should().Be("Ya posees este producto");
    }

    [Fact]
    public async Task PurchaseStoreItemUseCase_WithInsufficientCoins_ShouldThrowBusinessException()
    {
        // Arrange
        var storeRepositoryMock = new Mock<IStoreRepository>();
        var playerRepositoryMock = new Mock<IPlayerRepository>();
        
        const int playerId = 1;
        const int productId = 10;
        const decimal productPrice = 1500;
        const int playerCoins = 1000; // Insufficient coins

        var player = new PlayerProfile
        {
            Id = playerId,
            Name = "TestPlayer",
            Email = "test@test.com",
            Uid = "uid_test",
            Coins = playerCoins,
            LastLevelId = 1,
            Points = 100
        };

        var product = new StoreItem
        {
            Id = productId,
            Name = "Producto Caro",
            Description = "Descripción del producto caro",
            Price = productPrice,
            ImageUrl = "",
            ProductTypeId = 1,
            ProductTypeName = "Auto",
            Rarity = "Épico",
            IsOwned = false,
            Currency = "Coins"
        };

        playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        storeRepositoryMock
            .Setup(x => x.GetProductByIdAsync(productId, playerId))
            .ReturnsAsync(product);

        var useCase = new PurchaseStoreItemUseCase(storeRepositoryMock.Object, playerRepositoryMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => useCase.ExecuteAsync(playerId, productId));

        exception.Message.Should().Be("No tienes suficientes monedas");

        // Verify that purchase was not attempted
        storeRepositoryMock.Verify(x => x.PurchaseProductAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task PurchaseStoreItemUseCase_WithValidPurchase_ShouldReturnRemainingCoins()
    {
        // Arrange
        var storeRepositoryMock = new Mock<IStoreRepository>();
        var playerRepositoryMock = new Mock<IPlayerRepository>();
        
        const int playerId = 1;
        const int productId = 10;
        const decimal productPrice = 500;
        const int playerCoins = 1000;

        var player = new PlayerProfile
        {
            Id = playerId,
            Name = "TestPlayer",
            Email = "test@test.com",
            Uid = "uid_test",
            Coins = playerCoins,
            LastLevelId = 1,
            Points = 100
        };

        var product = new StoreItem
        {
            Id = productId,
            Name = "Producto Test",
            Description = "Descripción del producto test",
            Price = productPrice,
            ImageUrl = "",
            ProductTypeId = 1,
            ProductTypeName = "Auto",
            Rarity = "Común",
            IsOwned = false,
            Currency = "Coins"
        };

        playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        storeRepositoryMock
            .Setup(x => x.GetProductByIdAsync(productId, playerId))
            .ReturnsAsync(product);

        storeRepositoryMock
            .Setup(x => x.PurchaseProductAsync(playerId, productId, productPrice))
            .ReturnsAsync(true);

        var useCase = new PurchaseStoreItemUseCase(storeRepositoryMock.Object, playerRepositoryMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(playerId, productId);

        // Assert
        result.Should().Be(playerCoins - productPrice);

        // Verify repository calls
        playerRepositoryMock.Verify(x => x.GetByIdAsync(playerId), Times.Once);
        storeRepositoryMock.Verify(x => x.GetProductByIdAsync(productId, playerId), Times.Once);
        storeRepositoryMock.Verify(x => x.PurchaseProductAsync(playerId, productId, productPrice), Times.Once);
    }

    [Fact]
    public async Task PurchaseStoreItemUseCase_WithRepositoryFailure_ShouldThrowBusinessException()
    {
        // Arrange
        var storeRepositoryMock = new Mock<IStoreRepository>();
        var playerRepositoryMock = new Mock<IPlayerRepository>();
        
        const int playerId = 1;
        const int productId = 10;
        const decimal productPrice = 500;
        const int playerCoins = 1000;

        var player = new PlayerProfile
        {
            Id = playerId,
            Name = "TestPlayer",
            Email = "test@test.com",
            Uid = "uid_test",
            Coins = playerCoins,
            LastLevelId = 1,
            Points = 100
        };

        var product = new StoreItem
        {
            Id = productId,
            Name = "Producto Test",
            Description = "Descripción del producto test",
            Price = productPrice,
            ImageUrl = "",
            ProductTypeId = 1,
            ProductTypeName = "Auto",
            Rarity = "Común",
            IsOwned = false,
            Currency = "Coins"
        };

        playerRepositoryMock
            .Setup(x => x.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        storeRepositoryMock
            .Setup(x => x.GetProductByIdAsync(productId, playerId))
            .ReturnsAsync(product);

        storeRepositoryMock
            .Setup(x => x.PurchaseProductAsync(playerId, productId, productPrice))
            .ReturnsAsync(false); // Repository failure

        var useCase = new PurchaseStoreItemUseCase(storeRepositoryMock.Object, playerRepositoryMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => useCase.ExecuteAsync(playerId, productId));

        exception.Message.Should().Be("Error al procesar la compra");
    }

    #endregion
}