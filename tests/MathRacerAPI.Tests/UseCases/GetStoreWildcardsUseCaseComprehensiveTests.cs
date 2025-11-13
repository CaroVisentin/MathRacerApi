using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios comprensivos para el caso de uso de obtener wildcards de la tienda
/// </summary>
public class GetStoreWildcardsUseCaseComprehensiveTests
{
    private readonly Mock<IWildcardRepository> _wildcardRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly GetStoreWildcardsUseCase _getStoreWildcardsUseCase;

    public GetStoreWildcardsUseCaseComprehensiveTests()
    {
        _wildcardRepositoryMock = new Mock<IWildcardRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _getStoreWildcardsUseCase = new GetStoreWildcardsUseCase(
            _wildcardRepositoryMock.Object,
            _playerRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPlayerId_ShouldThrowNotFoundException()
    {
        // Arrange
        const int invalidPlayerId = 999;

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidPlayerId))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await _getStoreWildcardsUseCase
            .Invoking(x => x.ExecuteAsync(invalidPlayerId))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Jugador no encontrado");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidPlayer_ShouldReturnStoreWildcards()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var storeWildcards = CreateTestStoreWildcards();
        var playerWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard { WildcardId = 1, Quantity = 5 },
            new PlayerWildcard { WildcardId = 2, Quantity = 3 }
            // Player doesn't have wildcard 3
        };

        SetupMocks(player, storeWildcards, playerWildcards);

        // Act
        var result = await _getStoreWildcardsUseCase.ExecuteAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        var wildcard1 = result.First(w => w.Id == 1);
        wildcard1.Name.Should().Be("Double Points");
        wildcard1.Price.Should().Be(15);
        wildcard1.CurrentQuantity.Should().Be(5);

        var wildcard2 = result.First(w => w.Id == 2);
        wildcard2.Name.Should().Be("Skip Question");
        wildcard2.Price.Should().Be(25);
        wildcard2.CurrentQuantity.Should().Be(3);

        var wildcard3 = result.First(w => w.Id == 3);
        wildcard3.Name.Should().Be("Extra Time");
        wildcard3.Price.Should().Be(20);
        wildcard3.CurrentQuantity.Should().Be(0); // Player doesn't have this one
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPlayerWildcards_ShouldReturnZeroQuantities()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var storeWildcards = CreateTestStoreWildcards();
        var playerWildcards = new List<PlayerWildcard>(); // Player has no wildcards

        SetupMocks(player, storeWildcards, playerWildcards);

        // Act
        var result = await _getStoreWildcardsUseCase.ExecuteAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().OnlyContain(w => w.CurrentQuantity == 0);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyStore_ShouldReturnEmptyList()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var storeWildcards = new List<Wildcard>(); // Empty store
        var playerWildcards = new List<PlayerWildcard>();

        SetupMocks(player, storeWildcards, playerWildcards);

        // Act
        var result = await _getStoreWildcardsUseCase.ExecuteAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithAllWildcardsOwned_ShouldShowCorrectQuantities()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var storeWildcards = CreateTestStoreWildcards();
        var playerWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard { WildcardId = 1, Quantity = 99 }, // Max quantity
            new PlayerWildcard { WildcardId = 2, Quantity = 50 },
            new PlayerWildcard { WildcardId = 3, Quantity = 1 }
        };

        SetupMocks(player, storeWildcards, playerWildcards);

        // Act
        var result = await _getStoreWildcardsUseCase.ExecuteAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        var wildcard1 = result.First(w => w.Id == 1);
        wildcard1.CurrentQuantity.Should().Be(99);

        var wildcard2 = result.First(w => w.Id == 2);
        wildcard2.CurrentQuantity.Should().Be(50);

        var wildcard3 = result.First(w => w.Id == 3);
        wildcard3.CurrentQuantity.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreserveWildcardProperties()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var storeWildcards = new List<Wildcard>
        {
            new Wildcard
            {
                Id = 100,
                Name = "Special Wildcard",
                Description = "A very special wildcard with unique powers",
                Price = 99.5
            }
        };
        var playerWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard { WildcardId = 100, Quantity = 7 }
        };

        SetupMocks(player, storeWildcards, playerWildcards);

        // Act
        var result = await _getStoreWildcardsUseCase.ExecuteAsync(playerId);

        // Assert
        result.Should().HaveCount(1);
        var storeWildcard = result.First();

        storeWildcard.Id.Should().Be(100);
        storeWildcard.Name.Should().Be("Special Wildcard");
        storeWildcard.Description.Should().Be("A very special wildcard with unique powers");
        storeWildcard.Price.Should().Be(99); // Converted to int
        storeWildcard.CurrentQuantity.Should().Be(7);
    }

    [Fact]
    public async Task ExecuteAsync_WithDecimalPrices_ShouldConvertToInteger()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var storeWildcards = new List<Wildcard>
        {
            new Wildcard { Id = 1, Name = "Test1", Price = 15.7 },
            new Wildcard { Id = 2, Name = "Test2", Price = 25.3 },
            new Wildcard { Id = 3, Name = "Test3", Price = 30.0 }
        };
        var playerWildcards = new List<PlayerWildcard>();

        SetupMocks(player, storeWildcards, playerWildcards);

        // Act
        var result = await _getStoreWildcardsUseCase.ExecuteAsync(playerId);

        // Assert
        result.Should().HaveCount(3);
        result[0].Price.Should().Be(15); // 15.7m -> 15
        result[1].Price.Should().Be(25); // 25.3m -> 25
        result[2].Price.Should().Be(30); // 30.0m -> 30
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryMethodsOnce()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var storeWildcards = CreateTestStoreWildcards();
        var playerWildcards = new List<PlayerWildcard>();

        SetupMocks(player, storeWildcards, playerWildcards);

        // Act
        await _getStoreWildcardsUseCase.ExecuteAsync(playerId);

        // Assert
        _playerRepositoryMock.Verify(x => x.GetByIdAsync(playerId), Times.Once);
        _wildcardRepositoryMock.Verify(x => x.GetStoreWildcardsAsync(), Times.Once);
        _wildcardRepositoryMock.Verify(x => x.GetPlayerWildcardsAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPartialWildcardOwnership_ShouldHandleMixedScenarios()
    {
        // Arrange
        const int playerId = 1;
        var player = CreateTestPlayer(playerId);
        var storeWildcards = new List<Wildcard>
        {
            new Wildcard { Id = 1, Name = "Owned", Price = 10.0 },
            new Wildcard { Id = 2, Name = "Not Owned", Price = 20.0 },
            new Wildcard { Id = 3, Name = "Partially Owned", Price = 30.0 },
            new Wildcard { Id = 4, Name = "Max Owned", Price = 40.0 }
        };
        var playerWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard { WildcardId = 1, Quantity = 10 },
            // WildcardId 2 not owned
            new PlayerWildcard { WildcardId = 3, Quantity = 45 },
            new PlayerWildcard { WildcardId = 4, Quantity = 99 }
        };

        SetupMocks(player, storeWildcards, playerWildcards);

        // Act
        var result = await _getStoreWildcardsUseCase.ExecuteAsync(playerId);

        // Assert
        result.Should().HaveCount(4);

        result.First(w => w.Id == 1).CurrentQuantity.Should().Be(10);
        result.First(w => w.Id == 2).CurrentQuantity.Should().Be(0);
        result.First(w => w.Id == 3).CurrentQuantity.Should().Be(45);
        result.First(w => w.Id == 4).CurrentQuantity.Should().Be(99);
    }

    #region Helper Methods

    private static PlayerProfile CreateTestPlayer(int id)
    {
        return new PlayerProfile
        {
            Id = id,
            Name = $"Player{id}",
            Email = $"player{id}@test.com",
            Coins = 1000,
            Points = 500,
            LastLevelId = 5
        };
    }

    private static List<Wildcard> CreateTestStoreWildcards()
    {
        return new List<Wildcard>
        {
            new Wildcard
            {
                Id = 1,
                Name = "Double Points",
                Description = "Doubles your points for the next correct answer",
                Price = 15.0
            },
            new Wildcard
            {
                Id = 2,
                Name = "Skip Question",
                Description = "Skip the current question without penalty",
                Price = 25.0
            },
            new Wildcard
            {
                Id = 3,
                Name = "Extra Time",
                Description = "Adds 10 seconds to the question timer",
                Price = 20.0
            }
        };
    }

    private void SetupMocks(PlayerProfile player, List<Wildcard> storeWildcards, List<PlayerWildcard> playerWildcards)
    {
        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(player.Id))
            .ReturnsAsync(player);

        _wildcardRepositoryMock
            .Setup(x => x.GetStoreWildcardsAsync())
            .ReturnsAsync(storeWildcards);

        _wildcardRepositoryMock
            .Setup(x => x.GetPlayerWildcardsAsync(player.Id))
            .ReturnsAsync(playerWildcards);
    }

    #endregion
}