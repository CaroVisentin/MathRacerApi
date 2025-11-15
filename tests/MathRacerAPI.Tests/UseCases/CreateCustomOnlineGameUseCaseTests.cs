using Xunit;
using Moq;
using FluentAssertions;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para CreateCustomOnlineGameUseCase
/// </summary>
public class CreateCustomOnlineGameUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IWorldRepository> _worldRepositoryMock;
    private readonly Mock<ILevelRepository> _levelRepositoryMock;
    private readonly GetQuestionsUseCase _getQuestionsUseCase; 
    private readonly Mock<IPowerUpService> _powerUpServiceMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly CreateCustomOnlineGameUseCase _useCase;

    public CreateCustomOnlineGameUseCaseTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _worldRepositoryMock = new Mock<IWorldRepository>();
        _levelRepositoryMock = new Mock<ILevelRepository>();
        _getQuestionsUseCase = new GetQuestionsUseCase(); 
        _powerUpServiceMock = new Mock<IPowerUpService>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();

        _useCase = new CreateCustomOnlineGameUseCase(
            _gameRepositoryMock.Object,
            _worldRepositoryMock.Object,
            _levelRepositoryMock.Object,
            _getQuestionsUseCase, 
            _powerUpServiceMock.Object,
            _playerRepositoryMock.Object
        );
    }

    #region Validación de Contraseñas

    [Fact]
    public async Task ExecuteAsync_PrivateGameWithoutPassword_ShouldThrowValidationException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid, gameName, true, null, "FACIL", "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*contraseña*requerida*");
    }

    [Fact]
    public async Task ExecuteAsync_PrivateGameWithEmptyPassword_ShouldThrowValidationException()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Test Game";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid, gameName, true, "   ", "FACIL", "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*contraseña*requerida*");
    }

    [Fact]
    public async Task ExecuteAsync_PublicGameWithoutPassword_ShouldSucceed()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Public Game";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid, gameName, false, null, "FACIL", "MAYOR"
        );

        // Assert
        result.Should().NotBeNull();
        result.IsPrivate.Should().BeFalse();
        result.Password.Should().BeNull();
    }

    #endregion

    #region Validación de Nombre de Partida

    [Fact]
    public async Task ExecuteAsync_EmptyGameName_ShouldThrowValidationException()
    {
        // Arrange
        var firebaseUid = "test-uid";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid, "", false, null, "FACIL", "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*nombre*requerido*");
    }

    [Fact]
    public async Task ExecuteAsync_NullGameName_ShouldThrowValidationException()
    {
        // Arrange
        var firebaseUid = "test-uid";

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid, null!, false, null, "FACIL", "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*nombre*requerido*");
    }

    #endregion

    #region Validación de Jugador

    [Fact]
    public async Task ExecuteAsync_PlayerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var firebaseUid = "non-existent-uid";
        var gameName = "Test Game";

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(
            firebaseUid, gameName, false, null, "FACIL", "MAYOR"
        );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*jugador*");
    }

    #endregion

    #region Creación Exitosa

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldCreateGameWithoutPlayers()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var gameName = "Epic Battle";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid, gameName, false, null, "MEDIO", "MAYOR"
        );

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(gameName);
        result.Status.Should().Be(GameStatus.WaitingForPlayers);
        result.PowerUpsEnabled.Should().BeTrue();
        result.MaxPowerUpsPerPlayer.Should().Be(3);
        result.ExpectedResult.Should().Be("MAYOR");
        result.Players.Should().BeEmpty();
        result.CreatorPlayerId.Should().BeNull();
        result.Questions.Should().HaveCount(10);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateQuestionsBasedOnDifficulty()
    {
        // Arrange
        var firebaseUid = "test-uid";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid, "Test Game", false, null, "DIFICIL", "MENOR"
        );

        // Assert
        result.Questions.Should().HaveCount(10);
        result.Questions.Should().OnlyContain(q => !string.IsNullOrEmpty(q.Equation));
        result.ExpectedResult.Should().Be("MENOR");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveGameToRepository()
    {
        // Arrange
        var firebaseUid = "test-uid";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        await _useCase.ExecuteAsync(
            firebaseUid, "Test Game", false, null, "FACIL", "MAYOR"
        );

        // Assert
        _gameRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Game>(g => 
                g.Name == "Test Game" && 
                g.Status == GameStatus.WaitingForPlayers &&
                g.Players.Count == 0
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_PrivateGame_ShouldStorePassword()
    {
        // Arrange
        var firebaseUid = "test-uid";
        var password = "secret123";

        SetupSuccessfulGameCreation(firebaseUid);

        // Act
        var result = await _useCase.ExecuteAsync(
            firebaseUid, "Private Game", true, password, "FACIL", "MAYOR"
        );

        // Assert
        result.IsPrivate.Should().BeTrue();
        result.Password.Should().Be(password);
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulGameCreation(string firebaseUid)
    {
        var profile = new PlayerProfile
        {
            Id = 1,
            Name = "Test Player",
            Uid = firebaseUid,
            Email = "test@example.com"
        };

        _playerRepositoryMock
            .Setup(x => x.GetByUidAsync(firebaseUid))
            .ReturnsAsync(profile);

        _gameRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game g) => g);
    }

    #endregion
}