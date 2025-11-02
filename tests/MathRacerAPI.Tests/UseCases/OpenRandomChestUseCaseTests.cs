using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static MathRacerAPI.Domain.Models.ChestItem;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de apertura de cofres aleatorios
/// </summary>
public class OpenRandomChestUseCaseTests
{
    private readonly Mock<IChestRepository> _mockChestRepository;
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly OpenRandomChestUseCase _useCase;

    public OpenRandomChestUseCaseTests()
    {
        _mockChestRepository = new Mock<IChestRepository>();
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _useCase = new OpenRandomChestUseCase(_mockChestRepository.Object, _mockPlayerRepository.Object);

        // Setup común para productos, wildcards y player
        SetupDefaultMocks();
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteAsync_WithValidUid_ShouldReturn3Items()
    {
        // Arrange
        var playerUid = "firebase-uid-123";

        // Act
        var result = await _useCase.ExecuteAsync(playerUid);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateRandomItemTypes()
    {
        // Arrange
        var playerUid = "firebase-uid-123";

        // Act
        var result = await _useCase.ExecuteAsync(playerUid);

        // Assert
        result.Items.Should().NotBeNullOrEmpty();
        result.Items.Should().OnlyContain(item => 
            item.Type == ChestItemType.Product ||
            item.Type == ChestItemType.Coins ||
            item.Type == ChestItemType.Wildcard);
    }

    [Fact]
    public async Task ExecuteAsync_WithNewProduct_ShouldAssignProductToPlayer()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerId = 1;
        var newProductId = 20;

        // Mock: Producto NO duplicado
        _mockChestRepository
            .Setup(r => r.PlayerHasProductAsync(playerId, newProductId))
            .ReturnsAsync(false);

        _mockChestRepository
            .Setup(r => r.GetRandomProductByRarityProbabilityAsync())
            .ReturnsAsync(new Product
            {
                Id = newProductId,
                Name = "Producto Nuevo",
                RarityId = 2
            });

        // Act
        await _useCase.ExecuteAsync(playerUid);

        // Assert
        _mockChestRepository.Verify(
            r => r.AssignProductsToPlayerAsync(
                playerId,
                It.Is<List<int>>(list => list.Count == 1 && list.Contains(newProductId)),
                false), // NO activo por defecto en cofres aleatorios
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateProduct_ShouldGiveCompensation()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerId = 1;
        var productId = 10;

        // Mock: Producto duplicado
        _mockChestRepository
            .Setup(r => r.PlayerHasProductAsync(playerId, productId))
            .ReturnsAsync(true);

        _mockChestRepository
            .Setup(r => r.GetRandomProductByRarityProbabilityAsync())
            .ReturnsAsync(CreateProduct(productId, "Auto Duplicado", 2)); // Rareza 2 = 150 coins

        // Act
        var result = await _useCase.ExecuteAsync(playerUid);

        // Assert
        var duplicateItem = result.Items.FirstOrDefault(i => 
            i.Type == ChestItemType.Product && i.CompensationCoins.HasValue);

        if (duplicateItem != null)
        {
            duplicateItem.CompensationCoins.Should().BeGreaterThan(0);
            
            // Verificar que se agregaron monedas de compensación
            _mockChestRepository.Verify(
                r => r.AddCoinsToPlayerAsync(playerId, It.IsAny<int>()),
                Times.AtLeastOnce);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithCoins_ShouldAddCoinsToPlayer()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerId = 1;

        // Act
        await _useCase.ExecuteAsync(playerUid);

        // Assert
        _mockChestRepository.Verify(
            r => r.AddCoinsToPlayerAsync(playerId, It.IsInRange(100, 1000, Moq.Range.Inclusive)),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WithWildcard_ShouldAddWildcardToPlayer()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerId = 1;

        // Act
        await _useCase.ExecuteAsync(playerUid);

        // Assert
        _mockChestRepository.Verify(
            r => r.AddWildcardsToPlayerAsync(
                playerId, 
                It.IsAny<int>(), 
                It.IsInRange(1, 3, Moq.Range.Inclusive)),
            Times.AtLeastOnce); 
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotAssignDuplicateProduct()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerId = 1;

        // Mock: Todos los productos son duplicados
        _mockChestRepository
            .Setup(r => r.PlayerHasProductAsync(playerId, It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _useCase.ExecuteAsync(playerUid);

        // Assert
        // NO debe llamar a AssignProductsToPlayerAsync para productos duplicados
        _mockChestRepository.Verify(
            r => r.AssignProductsToPlayerAsync(
                playerId,
                It.IsAny<List<int>>(),
                false),
            Times.Never);
    }

    #endregion

    #region Validation Tests - Invalid Inputs

    [Fact]
    public async Task ExecuteAsync_WithNonExistentPlayer_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var playerUid = "non-existent-uid";

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        var act = async () => await _useCase.ExecuteAsync(playerUid);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no encontrado*");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullUid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        string? nullUid = null;

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(It.IsAny<string>()))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        var act = async () => await _useCase.ExecuteAsync(nullUid!);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Probability Tests

    [Fact]
    public async Task ExecuteAsync_WithMultipleCalls_ShouldGenerateVariedResults()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var results = new List<Chest>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            results.Add(await _useCase.ExecuteAsync(playerUid));
        }

        // Assert
        // Verificar que hay variedad en los tipos de items
        var allItems = results.SelectMany(r => r.Items).ToList();
        
        allItems.Should().Contain(i => i.Type == ChestItemType.Coins);
        // Puede haber productos o wildcards dependiendo del random
        allItems.Count.Should().Be(30); // 10 cofres * 3 items
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectCoinRanges()
    {
        // Arrange
        var playerUid = "firebase-uid-123";

        // Act
        var result = await _useCase.ExecuteAsync(playerUid);

        // Assert
        var coinItems = result.Items.Where(i => i.Type == ChestItemType.Coins);
        
        foreach (var item in coinItems)
        {
            item.Quantity.Should().BeGreaterOrEqualTo(100);
            item.Quantity.Should().BeLessOrEqualTo(1000);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectWildcardRanges()
    {
        // Arrange
        var playerUid = "firebase-uid-123";

        // Act
        var result = await _useCase.ExecuteAsync(playerUid);

        // Assert
        var wildcardItems = result.Items.Where(i => i.Type == ChestItemType.Wildcard);
        
        foreach (var item in wildcardItems)
        {
            item.Quantity.Should().BeGreaterOrEqualTo(1);
            item.Quantity.Should().BeLessOrEqualTo(3);
        }
    }

    #endregion

    #region Helper Methods

    private void SetupDefaultMocks()
    {
        var playerProfile = new PlayerProfile
        {
            Id = 1,
            Name = "Test Player",
            Email = "test@example.com",
            Uid = "firebase-uid-123",
            Coins = 500,
            Points = 100
        };

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(It.IsAny<string>()))
            .ReturnsAsync(playerProfile);

        _mockChestRepository
            .Setup(r => r.GetRandomProductByRarityProbabilityAsync())
            .ReturnsAsync(CreateProduct(10, "Auto Aleatorio", 2));

        _mockChestRepository
            .Setup(r => r.GetRandomWildcardAsync())
            .ReturnsAsync(new Wildcard
            {
                Id = 1,
                Name = "50/50",
                Description = "Elimina 2 opciones"
            });

        _mockChestRepository
            .Setup(r => r.PlayerHasProductAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        _mockChestRepository
            .Setup(r => r.AssignProductsToPlayerAsync(
                It.IsAny<int>(), 
                It.IsAny<List<int>>(), 
                It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        _mockChestRepository
            .Setup(r => r.AddCoinsToPlayerAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockChestRepository
            .Setup(r => r.AddWildcardsToPlayerAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);
    }

    private static Product CreateProduct(int id, string name, int rarityId)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = $"Descripción de {name}",
            ProductType = 1,
            RarityId = rarityId,
            RarityName = GetRarityName(rarityId),
            RarityColor = GetRarityColor(rarityId)
        };
    }

    private static string GetRarityName(int rarityId) => rarityId switch
    {
        1 => "Común",
        2 => "Poco Común",
        3 => "Raro",
        4 => "Épico",
        5 => "Legendario",
        _ => "Desconocido"
    };

    private static string GetRarityColor(int rarityId) => rarityId switch
    {
        1 => "#FFFFFF",
        2 => "#1EFF00",
        3 => "#007BFF",
        4 => "#A335EE",
        5 => "#FFA500",
        _ => "#000000"
    };

    #endregion
}