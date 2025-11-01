using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Presentation.Controllers;
using MathRacerAPI.Presentation.DTOs.Solo;

namespace MathRacerAPI.Tests.Controllers;

/// <summary>
/// Tests unitarios para el SoloController
/// </summary>
public class SoloControllerTests
{
    private readonly Mock<ISoloGameRepository> _soloGameRepositoryMock;
    private readonly Mock<IEnergyRepository> _energyRepositoryMock;
    private readonly Mock<ILevelRepository> _levelRepositoryMock;
    private readonly Mock<IWorldRepository> _worldRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    
    private readonly SoloController _controller;

    public SoloControllerTests()
    {
        // Mockear repositorios en lugar de Use Cases
        _soloGameRepositoryMock = new Mock<ISoloGameRepository>();
        _energyRepositoryMock = new Mock<IEnergyRepository>();
        _levelRepositoryMock = new Mock<ILevelRepository>();
        _worldRepositoryMock = new Mock<IWorldRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();

        // Crear Use Cases reales con repositorios mockeados
        var getQuestionsUseCase = new GetQuestionsUseCase();
        var getPlayerByIdUseCase = new GetPlayerByIdUseCase(_playerRepositoryMock.Object);
        
        var startSoloGameUseCase = new StartSoloGameUseCase(
            _soloGameRepositoryMock.Object,
            _energyRepositoryMock.Object,
            _levelRepositoryMock.Object,
            _worldRepositoryMock.Object,
            _productRepositoryMock.Object,
            getQuestionsUseCase,
            getPlayerByIdUseCase);

        var getSoloGameStatusUseCase = new GetSoloGameStatusUseCase(
            _soloGameRepositoryMock.Object);

        // Crear GrantLevelRewardUseCase con repositorio mockeado
        var grantLevelRewardUseCase = new GrantLevelRewardUseCase(
            _playerRepositoryMock.Object);

        // Pasar GrantLevelRewardUseCase a SubmitSoloAnswerUseCase
        var submitSoloAnswerUseCase = new SubmitSoloAnswerUseCase(
            _soloGameRepositoryMock.Object,
            _energyRepositoryMock.Object,
            grantLevelRewardUseCase); 

        // Crear controller con Use Cases reales
        _controller = new SoloController(
            startSoloGameUseCase,
            getSoloGameStatusUseCase,
            submitSoloAnswerUseCase);

        // Configurar HttpContext con FirebaseUid
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region StartGame Tests

    [Fact]
    public async Task StartGame_WhenUidIsNull_ShouldReturnUnauthorized()
    {
        // Arrange
        const int levelId = 1;
        _controller.HttpContext.Items["FirebaseUid"] = null;

        // Act
        var result = await _controller.StartGame(levelId);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task StartGame_WhenValidRequest_ShouldReturnOkWithGameData()
    {
        // Arrange
        const int levelId = 1;
        const string uid = "test-uid-123";
        _controller.HttpContext.Items["FirebaseUid"] = uid;

        var player = CreateTestPlayer(uid);
        var level = CreateTestLevel(levelId);
        var world = CreateTestWorld();
        var playerProducts = CreateTestPlayerProducts();
        var machineProducts = CreateTestMachineProducts();

        // Setup mocks para los repositorios
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
            .ReturnsAsync((SoloGame game) => 
            {
                game.Id = 123;
                return game;
            });

        // Act
        var result = await _controller.StartGame(levelId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<StartSoloGameResponseDto>();
        
        var responseDto = okResult.Value as StartSoloGameResponseDto;
        responseDto!.GameId.Should().Be(123);
        responseDto.PlayerId.Should().Be(player.Id);
        responseDto.LevelId.Should().Be(levelId);
        responseDto.LivesRemaining.Should().Be(3);
        
        _soloGameRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    [Fact]
    public async Task StartGame_WhenNoEnergy_ShouldThrowBusinessException()
    {
        // Arrange
        const int levelId = 1;
        const string uid = "test-uid-123";
        _controller.HttpContext.Items["FirebaseUid"] = uid;

        var player = CreateTestPlayer(uid);

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(uid))
            .ReturnsAsync(player);

        _energyRepositoryMock
            .Setup(x => x.HasEnergyAsync(player.Id))
            .ReturnsAsync(false); // Sin energía

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _controller.StartGame(levelId));
    }

    #endregion

    #region GetGameStatus Tests

    [Fact]
    public async Task GetGameStatus_WhenUidIsNull_ShouldReturnUnauthorized()
    {
        // Arrange
        const int gameId = 1;
        _controller.HttpContext.Items["FirebaseUid"] = null;

        // Act
        var result = await _controller.GetGameStatus(gameId);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetGameStatus_WhenValidRequest_ShouldReturnOkWithGameStatus()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        _controller.HttpContext.Items["FirebaseUid"] = uid;

        var game = CreateTestSoloGame(uid);
        game.Id = gameId;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.GetGameStatus(gameId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<SoloGameStatusResponseDto>();
        
        var responseDto = okResult.Value as SoloGameStatusResponseDto;
        responseDto!.GameId.Should().Be(gameId);
        responseDto.Status.Should().Be(SoloGameStatus.InProgress.ToString());
    }

    [Fact]
    public async Task GetGameStatus_WhenGameNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const int gameId = 999;
        const string uid = "test-uid-123";
        _controller.HttpContext.Items["FirebaseUid"] = uid;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync((SoloGame?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetGameStatus(gameId));
    }

    [Fact]
    public async Task GetGameStatus_WhenCalledTooEarly_ShouldThrowValidationException()
    {
        // Arrange
        const int gameId = 1;
        const string uid = "test-uid-123";
        _controller.HttpContext.Items["FirebaseUid"] = uid;

        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.LastAnswerTime = DateTime.UtcNow; // Acaba de responder
        game.ReviewTimeSeconds = 3;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _controller.GetGameStatus(gameId));
    }

    #endregion

    #region SubmitAnswer Tests

    [Fact]
    public async Task SubmitAnswer_WhenUidIsNull_ShouldReturnUnauthorized()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        _controller.HttpContext.Items["FirebaseUid"] = null;

        // Act
        var result = await _controller.SubmitAnswer(gameId, answer);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task SubmitAnswer_WhenCorrectAnswer_ShouldReturnOkWithCorrectFeedback()
    {
        // Arrange
        const int gameId = 1;
        const int answer = 5;
        const string uid = "test-uid-123";
        _controller.HttpContext.Items["FirebaseUid"] = uid;

        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.Questions[0].CorrectAnswer = answer;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.GetByIdAsync(game.PlayerId))
            .ReturnsAsync(CreateTestPlayer(uid));

        _playerRepositoryMock
            .Setup(x => x.AddCoinsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _playerRepositoryMock
            .Setup(x => x.UpdateLastLevelAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SubmitAnswer(gameId, answer);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var responseDto = okResult!.Value as SubmitSoloAnswerResponseDto;
        
        responseDto!.IsCorrect.Should().BeTrue();
        responseDto.CorrectAnswer.Should().Be(answer);
        responseDto.WaitTimeSeconds.Should().Be(3);
        
        _soloGameRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_WhenIncorrectAnswer_ShouldReturnOkWithIncorrectFeedback()
    {
        // Arrange
        const int gameId = 1;
        const int incorrectAnswer = 3;
        const int correctAnswer = 5;
        const string uid = "test-uid-123";
        _controller.HttpContext.Items["FirebaseUid"] = uid;

        var game = CreateTestSoloGame(uid);
        game.Id = gameId;
        game.Questions[0].CorrectAnswer = correctAnswer;

        _soloGameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _soloGameRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SubmitAnswer(gameId, incorrectAnswer);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var responseDto = okResult!.Value as SubmitSoloAnswerResponseDto;
        
        responseDto!.IsCorrect.Should().BeFalse();
        responseDto.CorrectAnswer.Should().Be(correctAnswer);
        responseDto.LivesRemaining.Should().Be(2);
    }

    #endregion

    #region Helper Methods

    private static PlayerProfile CreateTestPlayer(string uid)
    {
        return new PlayerProfile
        {
            Id = 100,
            Uid = uid,
            Name = "TestPlayer",
            Email = "test@test.com",
            LastLevelId = 0, 
            Coins = 0,  
            Points = 0 
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
            new PlayerProduct { ProductId = 1, Name = "Auto Rojo", ProductTypeId = 1, ProductTypeName = "Auto" },
            new PlayerProduct { ProductId = 2, Name = "Personaje", ProductTypeId = 2, ProductTypeName = "Personaje" },
            new PlayerProduct { ProductId = 3, Name = "Fondo", ProductTypeId = 3, ProductTypeName = "Fondo" }
        };
    }

    private static List<PlayerProduct> CreateTestMachineProducts()
    {
        return new List<PlayerProduct>
        {
            new PlayerProduct { ProductId = 4, Name = "Auto Azul", ProductTypeId = 1, ProductTypeName = "Auto" },
            new PlayerProduct { ProductId = 5, Name = "Personaje 2", ProductTypeId = 2, ProductTypeName = "Personaje" },
            new PlayerProduct { ProductId = 6, Name = "Fondo 2", ProductTypeId = 3, ProductTypeName = "Fondo" }
        };
    }

    private static SoloGame CreateTestSoloGame(string uid)
    {
        return new SoloGame
        {
            Id = 1,
            PlayerId = 100,
            PlayerUid = uid,
            PlayerName = "TestPlayer",
            LevelId = 1,
            WorldId = 1,
            PlayerPosition = 0,
            LivesRemaining = 3,
            CorrectAnswers = 0,
            CurrentQuestionIndex = 0,
            MachinePosition = 0,
            Questions = CreateTestQuestions(),
            TotalQuestions = 10,
            TimePerEquation = 10,
            GameStartedAt = DateTime.UtcNow,
            LastAnswerTime = null,
            ReviewTimeSeconds = 3,
            Status = SoloGameStatus.InProgress,
            PlayerProducts = CreateTestPlayerProducts(),
            MachineProducts = CreateTestMachineProducts()
        };
    }

    private static List<Question> CreateTestQuestions()
    {
        return new List<Question>
        {
            new Question
            {
                Id = 1,
                Equation = "y = 2*x + 3",
                CorrectAnswer = 5,
                Options = new List<int> { 3, 5, 7, 9 }
            },
            new Question
            {
                Id = 2,
                Equation = "y = x - 1",
                CorrectAnswer = 4,
                Options = new List<int> { 2, 4, 6, 8 }
            },
            new Question
            {
                Id = 3,
                Equation = "y = 3*x",
                CorrectAnswer = 6,
                Options = new List<int> { 3, 6, 9, 12 }
            }
        };
    }

    #endregion
}