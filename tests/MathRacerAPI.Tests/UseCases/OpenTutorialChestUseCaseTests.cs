using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de apertura del cofre tutorial
/// </summary>
public class OpenTutorialChestUseCaseTests
{
    private readonly Mock<IChestRepository> _mockChestRepository;
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly OpenTutorialChestUseCase _useCase;

    public OpenTutorialChestUseCaseTests()
    {
        _mockChestRepository = new Mock<IChestRepository>();
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _useCase = new OpenTutorialChestUseCase(_mockChestRepository.Object, _mockPlayerRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var playerUid = "non-existent-uid";

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        var act = async () => await _useCase.ExecuteAsync(playerUid);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*no encontrado*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerAlreadyHasProducts_ShouldThrowBusinessException()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerProfile = new PlayerProfile
        {
            Id = 1,
            Uid = playerUid,
            Name = "Test Player",
            Email = "test@test.com"
        };

        var existingProducts = new List<PlayerProduct>
        {
            new PlayerProduct { ProductId = 1, Name = "Auto", ProductTypeId = 1 },
            new PlayerProduct { ProductId = 2, Name = "Personaje", ProductTypeId = 2 },
            new PlayerProduct { ProductId = 3, Name = "Fondo", ProductTypeId = 3 }
        };

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);

        _mockChestRepository
            .Setup(r => r.GetActiveProductsByPlayerIdAsync(playerProfile.Id))
            .ReturnsAsync(existingProducts);

        // Act
        var act = async () => await _useCase.ExecuteAsync(playerUid);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*completado el tutorial*");

        // Verificar que NO se asignaron productos adicionales
        _mockChestRepository.Verify(
            r => r.AssignProductsToPlayerAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidFirstTime_ShouldReturnChestWith3Products()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerProfile = new PlayerProfile
        {
            Id = 1,
            Uid = playerUid,
            Name = "Test Player",
            Email = "test@test.com"
        };

        var tutorialProducts = new List<Product>
        {
            CreateProduct(1, "Auto Común", 1, 1),
            CreateProduct(2, "Personaje Común", 2, 1),
            CreateProduct(3, "Fondo Común", 3, 1)
        };

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);

        _mockChestRepository
            .Setup(r => r.GetActiveProductsByPlayerIdAsync(playerProfile.Id))
            .ReturnsAsync(new List<PlayerProduct>()); // Sin productos activos

        _mockChestRepository
            .Setup(r => r.GetTutorialProductsAsync())
            .ReturnsAsync(tutorialProducts);

        _mockChestRepository
            .Setup(r => r.AssignProductsToPlayerAsync(
                playerProfile.Id,
                It.IsAny<List<int>>(),
                true))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(playerUid);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.Items.Should().OnlyContain(item => item.Type == ChestItem.ChestItemType.Product);
        result.Items.Should().OnlyContain(item => item.Quantity == 1);

        // Verificar que se asignaron los productos como activos
        _mockChestRepository.Verify(
            r => r.AssignProductsToPlayerAsync(
                playerProfile.Id,
                It.Is<List<int>>(list => list.Count == 3 && list.Contains(1) && list.Contains(2) && list.Contains(3)),
                true),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidFirstTime_ShouldReturnProductsOfDifferentTypes()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerProfile = new PlayerProfile
        {
            Id = 1,
            Uid = playerUid,
            Name = "Test Player",
            Email = "test@test.com"
        };

        var tutorialProducts = new List<Product>
        {
            CreateProduct(1, "Auto Común", 1, 1),      // Tipo 1: Auto
            CreateProduct(2, "Personaje Común", 2, 1), // Tipo 2: Personaje
            CreateProduct(3, "Fondo Común", 3, 1)      // Tipo 3: Fondo
        };

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);

        _mockChestRepository
            .Setup(r => r.GetActiveProductsByPlayerIdAsync(playerProfile.Id))
            .ReturnsAsync(new List<PlayerProduct>());

        _mockChestRepository
            .Setup(r => r.GetTutorialProductsAsync())
            .ReturnsAsync(tutorialProducts);

        _mockChestRepository
            .Setup(r => r.AssignProductsToPlayerAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(playerUid);

        // Assert
        var productTypes = result.Items.Select(i => i.Product?.ProductType).Distinct().ToList();
        productTypes.Should().HaveCount(3);
        productTypes.Should().Contain(1);
        productTypes.Should().Contain(2);
        productTypes.Should().Contain(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerHasLessThan3Products_ShouldAllowOpening()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerProfile = new PlayerProfile
        {
            Id = 1,
            Uid = playerUid,
            Name = "Test Player",
            Email = "test@test.com"
        };

        // Jugador tiene solo 2 productos activos (caso edge)
        var existingProducts = new List<PlayerProduct>
        {
            new PlayerProduct { ProductId = 1, Name = "Auto", ProductTypeId = 1 },
            new PlayerProduct { ProductId = 2, Name = "Personaje", ProductTypeId = 2 }
        };

        var tutorialProducts = new List<Product>
        {
            CreateProduct(1, "Auto Común", 1, 1),
            CreateProduct(2, "Personaje Común", 2, 1),
            CreateProduct(3, "Fondo Común", 3, 1)
        };

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);

        _mockChestRepository
            .Setup(r => r.GetActiveProductsByPlayerIdAsync(playerProfile.Id))
            .ReturnsAsync(existingProducts); // Solo 2 productos

        _mockChestRepository
            .Setup(r => r.GetTutorialProductsAsync())
            .ReturnsAsync(tutorialProducts);

        _mockChestRepository
            .Setup(r => r.AssignProductsToPlayerAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(playerUid);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);

        // Verificar que se asignaron los productos
        _mockChestRepository.Verify(
            r => r.AssignProductsToPlayerAsync(playerProfile.Id, It.IsAny<List<int>>(), true),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerHasExactly3Products_ShouldThrowBusinessException()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerProfile = new PlayerProfile
        {
            Id = 1,
            Uid = playerUid,
            Name = "Test Player",
            Email = "test@test.com"
        };

        var existingProducts = new List<PlayerProduct>
        {
            new PlayerProduct { ProductId = 1, Name = "Auto", ProductTypeId = 1 },
            new PlayerProduct { ProductId = 2, Name = "Personaje", ProductTypeId = 2 },
            new PlayerProduct { ProductId = 3, Name = "Fondo", ProductTypeId = 3 }
        };

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);

        _mockChestRepository
            .Setup(r => r.GetActiveProductsByPlayerIdAsync(playerProfile.Id))
            .ReturnsAsync(existingProducts);

        // Act
        var act = async () => await _useCase.ExecuteAsync(playerUid);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetProductsAsActive()
    {
        // Arrange
        var playerUid = "firebase-uid-123";
        var playerProfile = new PlayerProfile
        {
            Id = 1,
            Uid = playerUid,
            Name = "Test Player",
            Email = "test@test.com"
        };

        var tutorialProducts = new List<Product>
        {
            CreateProduct(1, "Auto Común", 1, 1),
            CreateProduct(2, "Personaje Común", 2, 1),
            CreateProduct(3, "Fondo Común", 3, 1)
        };

        _mockPlayerRepository
            .Setup(r => r.GetByUidAsync(playerUid))
            .ReturnsAsync(playerProfile);

        _mockChestRepository
            .Setup(r => r.GetActiveProductsByPlayerIdAsync(playerProfile.Id))
            .ReturnsAsync(new List<PlayerProduct>());

        _mockChestRepository
            .Setup(r => r.GetTutorialProductsAsync())
            .ReturnsAsync(tutorialProducts);

        _mockChestRepository
            .Setup(r => r.AssignProductsToPlayerAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(playerUid);

        // Assert
        // Verificar que setAsActive = true
        _mockChestRepository.Verify(
            r => r.AssignProductsToPlayerAsync(
                playerProfile.Id,
                It.IsAny<List<int>>(),
                true), // ✅ Debe ser TRUE
            Times.Once);
    }

    #region Helper Methods

    private static Product CreateProduct(int id, string name, int productType, int rarityId)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = $"Descripción de {name}",
            ProductType = productType,
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