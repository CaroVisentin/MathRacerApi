using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests unitarios para el caso de uso de inicio de partida individual
/// </summary>
public class StartSoloGameUseCaseTests
{
    private readonly Mock<ISoloGameRepository> _soloGameRepositoryMock;
    private readonly Mock<IEnergyRepository> _energyRepositoryMock;
    private readonly Mock<ILevelRepository> _levelRepositoryMock;
    private readonly Mock<IWorldRepository> _worldRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly StartSoloGameUseCase _startSoloGameUseCase;
    private readonly Mock<IWildcardRepository> _wildcardRepositoryMock;

    private static int _nextPlayerId = 100;

    public StartSoloGameUseCaseTests()
    {
        _soloGameRepositoryMock = new Mock<ISoloGameRepository>();
        _energyRepositoryMock = new Mock<IEnergyRepository>();
        _levelRepositoryMock = new Mock<ILevelRepository>();
        _worldRepositoryMock = new Mock<IWorldRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _wildcardRepositoryMock = new Mock<IWildcardRepository>();

        // GetQuestionsUseCase es real (no tiene dependencias externas)
        _getQuestionsUseCase = new GetQuestionsUseCase();
        
        var getPlayerByIdUseCase = new GetPlayerByIdUseCase(_playerRepositoryMock.Object);

        _startSoloGameUseCase = new StartSoloGameUseCase(
            _soloGameRepositoryMock.Object,
            _energyRepositoryMock.Object,
            _levelRepositoryMock.Object,
            _worldRepositoryMock.Object,
            _productRepositoryMock.Object,
            _wildcardRepositoryMock.Object,
            _getQuestionsUseCase,
            getPlayerByIdUseCase);
    }

    #region Happy Path Tests

    [Fact]
    public async Task ExecuteAsync_WhenValidRequest_ShouldCreateAndReturnSoloGame()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;

        var player = CreateTestPlayer(uid);
        var level = CreateTestLevel(levelId);
        var world = CreateTestWorld();
        var playerProducts = CreateTestPlayerProducts();
        var machineProducts = CreateTestMachineProducts();

        SetupSuccessfulMocks(uid, levelId, player, level, world, playerProducts, machineProducts);

        // Act
        var result = await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(player.Id);
        result.PlayerUid.Should().Be(uid);
        result.PlayerName.Should().Be(player.Name);
        result.LevelId.Should().Be(levelId);
        result.WorldId.Should().Be(world.Id);
        result.TotalQuestions.Should().Be(10);
        result.LivesRemaining.Should().Be(3);
        result.Status.Should().Be(SoloGameStatus.InProgress);
        result.Questions.Should().HaveCount(15);
        result.Questions.Should().OnlyContain(q => q.Options.Count == world.OptionsCount);
        result.PlayerProducts.Should().HaveCount(3);
        result.MachineProducts.Should().HaveCount(3);

        VerifyAllRepositoriesCalled(uid, levelId, player.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidRequest_ShouldSetCorrectTimeValues()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;
        var beforeExecution = DateTime.UtcNow;

        SetupSuccessfulMocks(uid, levelId);

        // Act
        var result = await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        // Assert
        result.GameStartedAt.Should().BeCloseTo(beforeExecution, TimeSpan.FromSeconds(1));
        result.LastAnswerTime.Should().BeNull();
        result.GameFinishedAt.Should().BeNull();
        result.ReviewTimeSeconds.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidRequest_ShouldGenerateQuestionsWithCorrectConfiguration()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;

        var level = CreateTestLevel(levelId);
        level.TermsCount = 3;
        level.VariablesCount = 2;

        var world = CreateTestWorld();
        world.OptionsCount = 4;
        world.TimePerEquation = 15;

        SetupSuccessfulMocks(uid, levelId, level: level, world: world);

        // Act
        var result = await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        // Assert
        result.Questions.Should().HaveCount(15);
        result.Questions.Should().OnlyContain(q => q.Options.Count == 4);
        result.TimePerEquation.Should().Be(15);
        
        // Verificar que las preguntas tienen el formato correcto
        result.Questions.Should().OnlyContain(q => 
            q.Equation.StartsWith("y = ") && 
            q.Options.Contains(q.CorrectAnswer));
    }

    #endregion

    #region Validation Tests - Player

    [Fact]
    public async Task ExecuteAsync_WhenPlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const string uid = "non-existent-uid";
        const int levelId = 1;

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _startSoloGameUseCase.ExecuteAsync(uid, levelId));

        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _energyRepositoryMock.Verify(x => x.HasEnergyAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerHasNoEnergy_ShouldThrowBusinessException()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.HasEnergyAsync(player.Id))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => 
            _startSoloGameUseCase.ExecuteAsync(uid, levelId));

        exception.Message.Should().Contain("energía suficiente");
        
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _energyRepositoryMock.Verify(x => x.HasEnergyAsync(player.Id), Times.Once);
        _levelRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Validation Tests - Level and World

    [Fact]
    public async Task ExecuteAsync_WhenLevelNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 999;
        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.HasEnergyAsync(player.Id))
            .ReturnsAsync(true);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync((Level?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _startSoloGameUseCase.ExecuteAsync(uid, levelId));

        exception.Message.Should().Contain($"Nivel con ID {levelId}");
        
        _levelRepositoryMock.Verify(x => x.GetByIdAsync(levelId), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorldNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;
        var player = CreateTestPlayer(uid);
        var level = CreateTestLevel(levelId);
        level.WorldId = 999; // Mundo que no existe

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.HasEnergyAsync(player.Id))
            .ReturnsAsync(true);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(level);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<World>()); // Sin mundos

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _startSoloGameUseCase.ExecuteAsync(uid, levelId));

        exception.Message.Should().Contain($"Mundo con ID {level.WorldId}");
        
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Once);
    }

    #endregion

    #region Validation Tests - Products

    [Fact]
    public async Task ExecuteAsync_WhenPlayerHasLessThan3Products_ShouldThrowBusinessException()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;
        var player = CreateTestPlayer(uid);
        var level = CreateTestLevel(levelId);
        var world = CreateTestWorld();
        var incompleteProducts = new List<PlayerProduct>
        {
            new PlayerProduct { ProductId = 1, Name = "Auto", ProductTypeId = 1 }
            // Solo 1 producto en lugar de 3
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.HasEnergyAsync(player.Id))
            .ReturnsAsync(true);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(level);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<World> { world });

        _productRepositoryMock
            .Setup(x => x.GetActiveProductsByPlayerIdAsync(player.Id))
            .ReturnsAsync(incompleteProducts);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => 
            _startSoloGameUseCase.ExecuteAsync(uid, levelId));

        exception.Message.Should().Contain("3 productos activos");
        
        _productRepositoryMock.Verify(
            x => x.GetActiveProductsByPlayerIdAsync(player.Id), 
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMachineProductsLoadFails_ShouldThrowBusinessException()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;
        var player = CreateTestPlayer(uid);
        var level = CreateTestLevel(levelId);
        var world = CreateTestWorld();
        var playerProducts = CreateTestPlayerProducts();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.HasEnergyAsync(player.Id))
            .ReturnsAsync(true);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(level);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<World> { world });

        _productRepositoryMock
            .Setup(x => x.GetActiveProductsByPlayerIdAsync(player.Id))
            .ReturnsAsync(playerProducts);

        _productRepositoryMock
            .Setup(x => x.GetRandomProductsForMachineAsync())
            .ReturnsAsync(new List<PlayerProduct>()); // Sin productos para máquina

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => 
            _startSoloGameUseCase.ExecuteAsync(uid, levelId));

        exception.Message.Should().Contain("productos de la máquina");
        
        _productRepositoryMock.Verify(
            x => x.GetRandomProductsForMachineAsync(), 
            Times.Once);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldCallRepositoriesInCorrectOrder()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;
        var callOrder = new List<string>();

        SetupSuccessfulMocksWithCallTracking(uid, levelId, callOrder);

        // Act
        await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        // Assert
        callOrder.Should().ContainInOrder(
            "GetPlayer",
            "HasEnergy",
            "GetLevel",
            "GetWorlds",
            "GetPlayerProducts",
            "GetMachineProducts",
            "AddGame"
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSaveGameToRepository()
    {
        // Arrange
        const string uid = "test-uid-123";
        const int levelId = 1;
        SoloGame? savedGame = null;

        SetupSuccessfulMocks(uid, levelId);

        _soloGameRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<SoloGame>()))
            .Callback<SoloGame>(game => savedGame = game)
            .ReturnsAsync((SoloGame game) => game);

        // Act
        var result = await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        // Assert
        savedGame.Should().NotBeNull();
        savedGame.Should().BeSameAs(result);
        _soloGameRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ExecuteAsync_WithDifferentLevels_ShouldCreateGameSuccessfully(int levelId)
    {
        // Arrange
        const string uid = "test-uid-123";
        SetupSuccessfulMocks(uid, levelId);

        // Act
        var result = await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        // Assert
        result.Should().NotBeNull();
        result.LevelId.Should().Be(levelId);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentPlayers_ShouldCreateSeparateGames()
    {
        // Arrange
        const string uid1 = "player-1";
        const string uid2 = "player-2";
        const int levelId = 1;

        SetupSuccessfulMocks(uid1, levelId, player: CreateTestPlayer(uid1));
        SetupSuccessfulMocks(uid2, levelId, player: CreateTestPlayer(uid2));

        // Act
        var game1 = await _startSoloGameUseCase.ExecuteAsync(uid1, levelId);
        var game2 = await _startSoloGameUseCase.ExecuteAsync(uid2, levelId);

        // Assert
        game1.PlayerUid.Should().Be(uid1);
        game2.PlayerUid.Should().Be(uid2);
        game1.PlayerId.Should().NotBe(game2.PlayerId);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Configura todos los mocks para un flujo exitoso
    /// </summary>
    private void SetupSuccessfulMocks(
        string uid, 
        int levelId,
        PlayerProfile? player = null,
        Level? level = null,
        World? world = null,
        List<PlayerProduct>? playerProducts = null,
        List<PlayerProduct>? machineProducts = null)
    {
        player ??= CreateTestPlayer(uid);
        level ??= CreateTestLevel(levelId);
        world ??= CreateTestWorld();
        playerProducts ??= CreateTestPlayerProducts();
        machineProducts ??= CreateTestMachineProducts();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.HasEnergyAsync(player.Id))
            .ReturnsAsync(true);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .ReturnsAsync(level);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .ReturnsAsync(new List<World> { world });

        _productRepositoryMock
            .Setup(x => x.GetActiveProductsByPlayerIdAsync(player.Id))
            .ReturnsAsync(playerProducts);

        _productRepositoryMock
            .Setup(x => x.GetRandomProductsForMachineAsync())
            .ReturnsAsync(machineProducts);

        _soloGameRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<SoloGame>()))
            .ReturnsAsync((SoloGame game) => game);
    }

    /// <summary>
    /// Configura mocks con tracking de orden de llamadas
    /// </summary>
    private void SetupSuccessfulMocksWithCallTracking(string uid, int levelId, List<string> callOrder)
    {
        var player = CreateTestPlayer(uid);
        var level = CreateTestLevel(levelId);
        var world = CreateTestWorld();
        var playerProducts = CreateTestPlayerProducts();
        var machineProducts = CreateTestMachineProducts();

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .Callback(() => callOrder.Add("GetPlayer"))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.HasEnergyAsync(player.Id))
            .Callback(() => callOrder.Add("HasEnergy"))
            .ReturnsAsync(true);

        _levelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId))
            .Callback(() => callOrder.Add("GetLevel"))
            .ReturnsAsync(level);

        _worldRepositoryMock
            .Setup(x => x.GetAllWorldsAsync())
            .Callback(() => callOrder.Add("GetWorlds"))
            .ReturnsAsync(new List<World> { world });

        _productRepositoryMock
            .Setup(x => x.GetActiveProductsByPlayerIdAsync(player.Id))
            .Callback(() => callOrder.Add("GetPlayerProducts"))
            .ReturnsAsync(playerProducts);

        _productRepositoryMock
            .Setup(x => x.GetRandomProductsForMachineAsync())
            .Callback(() => callOrder.Add("GetMachineProducts"))
            .ReturnsAsync(machineProducts);

        _soloGameRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<SoloGame>()))
            .Callback(() => callOrder.Add("AddGame"))
            .ReturnsAsync((SoloGame game) => game);
    }

    /// <summary>
    /// Verifica que todos los repositorios fueron llamados correctamente
    /// </summary>
    private void VerifyAllRepositoriesCalled(string uid, int levelId, int playerId)
    {
        _playerRepositoryMock.Verify(x => x.GetByUidAsync(uid), Times.Once);
        _energyRepositoryMock.Verify(x => x.HasEnergyAsync(playerId), Times.Once);
        _levelRepositoryMock.Verify(x => x.GetByIdAsync(levelId), Times.Once);
        _worldRepositoryMock.Verify(x => x.GetAllWorldsAsync(), Times.Once);
        _productRepositoryMock.Verify(x => x.GetActiveProductsByPlayerIdAsync(playerId), Times.Once);
        _productRepositoryMock.Verify(x => x.GetRandomProductsForMachineAsync(), Times.Once);
        _soloGameRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    /// Generar IDs automáticamente con contador
    private static PlayerProfile CreateTestPlayer(string uid)
    {
        return new PlayerProfile
        {
            Id = Interlocked.Increment(ref _nextPlayerId),
            Uid = uid,
            Name = "TestPlayer",
            Email = "test@test.com"
        };
    }

    private static Level CreateTestLevel(int levelId)
    {
        return new Level
        {
            Id = levelId,
            WorldId = 1,
            Number = 1,
            TermsCount = 2,
            VariablesCount = 1,
            ResultType = "MAYOR"
        };
    }

    private static World CreateTestWorld()
    {
        return new World
        {
            Id = 1,
            Name = "Mundo 1",
            OptionsCount = 4,
            TimePerEquation = 10,
            OptionRangeMin = -10,
            OptionRangeMax = 10,
            NumberRangeMin = -10,
            NumberRangeMax = 10,
            Operations = new List<string> { "+", "-" }
        };
    }

    private static List<PlayerProduct> CreateTestPlayerProducts()
    {
        return new List<PlayerProduct>
        {
            new PlayerProduct 
            { 
                ProductId = 1, 
                Name = "Auto Rojo", 
                ProductTypeId = 1, 
                ProductTypeName = "Auto" 
            },
            new PlayerProduct 
            { 
                ProductId = 2, 
                Name = "Personaje Default", 
                ProductTypeId = 2, 
                ProductTypeName = "Personaje" 
            },
            new PlayerProduct 
            { 
                ProductId = 3, 
                Name = "Fondo Ciudad", 
                ProductTypeId = 3, 
                ProductTypeName = "Fondo" 
            }
        };
    }

    private static List<PlayerProduct> CreateTestMachineProducts()
    {
        return new List<PlayerProduct>
        {
            new PlayerProduct 
            { 
                ProductId = 4, 
                Name = "Auto Azul", 
                ProductTypeId = 1, 
                ProductTypeName = "Auto" 
            },
            new PlayerProduct 
            { 
                ProductId = 5, 
                Name = "Personaje Máquina", 
                ProductTypeId = 2, 
                ProductTypeName = "Personaje" 
            },
            new PlayerProduct 
            { 
                ProductId = 6, 
                Name = "Fondo Carrera", 
                ProductTypeId = 3, 
                ProductTypeName = "Fondo" 
            }
        };
    }

    #endregion
}