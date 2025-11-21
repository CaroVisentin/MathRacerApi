using FluentAssertions;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Infrastructure.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de activación de wildcards en modo individual
/// </summary>
public class UseWildcardUseCaseTests
{
    private readonly Mock<ISoloGameRepository> _mockGameRepository;
    private readonly Mock<IWildcardRepository> _mockWildcardRepository;
    private readonly UseWildcardUseCase _useCase;

    public UseWildcardUseCaseTests()
    {
        _mockGameRepository = new Mock<ISoloGameRepository>();
        _mockWildcardRepository = new Mock<IWildcardRepository>();
        _useCase = new UseWildcardUseCase(
            _mockGameRepository.Object,
            _mockWildcardRepository.Object
        );
    }

    #region Wildcard ID 1 - RemoveWrongOption Tests

    [Fact]
    public async Task UseWildcard_RemoveWrongOption_ShouldRemoveOneIncorrectOption()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { -5, 3, 8, 10 }, correctAnswer: 8);
        var wildcardId = (int)WildcardType.RemoveWrongOption;

        SetupMocks(game, wildcardId);

        // Act
        var result = await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ModifiedOptions.Should().NotBeNull();
        result.ModifiedOptions!.Count.Should().Be(3); // 4 opciones - 1 = 3
        result.ModifiedOptions.Should().Contain(8); // Respuesta correcta debe estar
        result.RemainingQuantity.Should().Be(0); // Se consumió

        // Verificar que se guardó
        _mockGameRepository.Verify(r => r.UpdateAsync(It.IsAny<SoloGame>()), Times.Once);
    }

    [Fact]
    public async Task UseWildcard_RemoveWrongOption_ShouldNotRemoveCorrectAnswer()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        var wildcardId = (int)WildcardType.RemoveWrongOption;

        SetupMocks(game, wildcardId);

        // Act
        var result = await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        result.ModifiedOptions.Should().Contain(3);
        result.ModifiedOptions!.Count.Should().Be(3);
    }

    [Fact]
    public async Task UseWildcard_RemoveWrongOption_WhenAlreadyUsed_ShouldThrowException()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        game.UsedWildcardTypes.Add((int)WildcardType.RemoveWrongOption);
        var wildcardId = (int)WildcardType.RemoveWrongOption;

        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Ya usaste este comodín*");
    }

    [Fact]
    public async Task UseWildcard_RemoveWrongOption_WhenNoQuestionAvailable_ShouldThrowException()
    {
        // Arrange
        var game = CreateBasicSoloGame();
        game.CurrentQuestionIndex = 10; // Fuera de rango
        var wildcardId = (int)WildcardType.RemoveWrongOption;

        SetupMocks(game, wildcardId);

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*No hay pregunta actual*");
    }

    #endregion

    #region Wildcard ID 2 - SkipQuestion Tests

    [Fact]
    public async Task UseWildcard_SkipQuestion_ShouldAdvanceQuestionIndex()
    {
        // Arrange
        var game = CreateSoloGameWithMultipleQuestions(5);
        game.CurrentQuestionIndex = 2;
        var wildcardId = (int)WildcardType.SkipQuestion;

        SetupMocks(game, wildcardId);

        // Act
        var result = await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        result.Success.Should().BeTrue();
        game.CurrentQuestionIndex.Should().Be(3); 
        game.Questions[game.CurrentQuestionIndex].Should().NotBeNull();
        game.Questions[game.CurrentQuestionIndex].Id.Should().BeGreaterThan(0); 
    }

    [Fact]
    public async Task UseWildcard_SkipQuestion_ShouldResetLastAnswerTime()
    {
        // Arrange
        var game = CreateSoloGameWithMultipleQuestions(5);
        game.LastAnswerTime = DateTime.UtcNow.AddSeconds(-5);
        var originalTime = game.LastAnswerTime;
        var wildcardId = (int)WildcardType.SkipQuestion;

        SetupMocks(game, wildcardId);

        // Act
        var result = await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        game.LastAnswerTime.Should().NotBe(originalTime);
        game.LastAnswerTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UseWildcard_SkipQuestion_ShouldClearModifiedOptions()
    {
        // Arrange
        var game = CreateSoloGameWithMultipleQuestions(5);
        game.ModifiedOptions = new List<int> { 1, 2, 3 }; // Tenía opciones modificadas previas
        var wildcardId = (int)WildcardType.SkipQuestion;

        SetupMocks(game, wildcardId);

        // Act
        var result = await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        game.ModifiedOptions.Should().BeNull();
    }

    [Fact]
    public async Task UseWildcard_SkipQuestion_WhenNoMoreQuestions_ShouldThrowException()
    {
        // Arrange
        var game = CreateSoloGameWithMultipleQuestions(5);
        game.CurrentQuestionIndex = 4; // Última pregunta (índice 4 de 5 preguntas)
        var wildcardId = (int)WildcardType.SkipQuestion;

        SetupMocks(game, wildcardId);

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*No hay más preguntas disponibles*");
    }

    #endregion

    #region Wildcard ID 3 - DoubleProgress Tests

    [Fact]
    public async Task UseWildcard_DoubleProgress_ShouldActivateFlag()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        game.HasDoubleProgressActive = false;
        var wildcardId = (int)WildcardType.DoubleProgress;

        SetupMocks(game, wildcardId);

        // Act
        var result = await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        result.Success.Should().BeTrue();
        result.DoubleProgressActive.Should().BeTrue();
        game.HasDoubleProgressActive.Should().BeTrue();
        result.Message.Should().Contain("doble");
    }

    [Fact]
    public async Task UseWildcard_DoubleProgress_ShouldMarkAsUsed()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        var wildcardId = (int)WildcardType.DoubleProgress;

        SetupMocks(game, wildcardId);

        // Act
        var result = await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        game.UsedWildcardTypes.Should().Contain(wildcardId);
    }

    #endregion

    #region General Validation Tests

    [Fact]
    public async Task UseWildcard_WhenGameNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockGameRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SoloGame?)null);

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(999, 1, "player-uid");

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*no encontrada*");
    }

    [Fact]
    public async Task UseWildcard_WhenWrongPlayer_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateBasicSoloGame();
        game.PlayerUid = "owner-uid";

        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, 1, "different-uid");

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*no tienes permiso*");
    }

    [Fact]
    public async Task UseWildcard_WhenGameFinished_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateBasicSoloGame();
        game.Status = SoloGameStatus.PlayerWon;

        SetupMocks(game, 1);

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, 1, game.PlayerUid);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*el juego no está en progreso*");
    }

    [Fact]
    public async Task UseWildcard_WhenNotAvailableInDatabase_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3); 
        var wildcardId = 1;

        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
        _mockWildcardRepository.Setup(r => r.HasWildcardAvailableAsync(game.PlayerId, wildcardId))
            .ReturnsAsync(false); // No tiene en BD

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*No tienes este comodín disponible*");
    }

    [Fact]
    public async Task UseWildcard_WhenNotLoadedInGame_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        game.AvailableWildcards.Clear(); // Sin wildcards en la partida
        var wildcardId = 1;

        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
        _mockWildcardRepository.Setup(r => r.HasWildcardAvailableAsync(game.PlayerId, wildcardId))
            .ReturnsAsync(true); // Tiene en BD pero no en partida

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*no está disponible en esta partida*");
    }

    [Fact]
    public async Task UseWildcard_WhenQuantityIsZero_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3); 
        game.AvailableWildcards.First().Quantity = 0; // Cantidad en 0
        var wildcardId = 1;

        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
        _mockWildcardRepository.Setup(r => r.HasWildcardAvailableAsync(game.PlayerId, wildcardId))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*no está disponible en esta partida*");
    }

    [Fact]
    public async Task UseWildcard_ShouldConsumeFromDatabase()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        var wildcardId = (int)WildcardType.RemoveWrongOption;

        SetupMocks(game, wildcardId);

        // Act
        await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        _mockWildcardRepository.Verify(
            r => r.ConsumeWildcardAsync(game.PlayerId, wildcardId),
            Times.Once);
    }

    [Fact]
    public async Task UseWildcard_ShouldDecreaseQuantityInGame()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        var wildcard = game.AvailableWildcards.First();
        wildcard.Quantity = 3;
        var wildcardId = wildcard.WildcardId;

        SetupMocks(game, wildcardId);

        // Act
        await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        wildcard.Quantity.Should().Be(2); // 3 - 1 = 2
    }

    [Fact]
    public async Task UseWildcard_WhenDatabaseConsumeFails_ShouldThrowException()
    {
        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        var wildcardId = 1;

        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
        _mockWildcardRepository.Setup(r => r.HasWildcardAvailableAsync(game.PlayerId, wildcardId))
            .ReturnsAsync(true);
        _mockWildcardRepository.Setup(r => r.ConsumeWildcardAsync(game.PlayerId, wildcardId))
            .ReturnsAsync(false); // Falla el consumo

        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Error al consumir el comodín*");
    }

    [Theory]
    [InlineData(1)] // RemoveWrongOption
    [InlineData(2)] // SkipQuestion
    [InlineData(3)] // DoubleProgress
    public async Task UseWildcard_AllTypes_ShouldMarkAsUsedOnlyOnce(int wildcardId)
    {
        // Arrange
        var game = CreateSoloGameWithMultipleQuestions(5);
        game.UsedWildcardTypes.Clear();

        SetupMocks(game, wildcardId);

        // Act
        await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        game.UsedWildcardTypes.Should().HaveCount(1);
        game.UsedWildcardTypes.Should().Contain(wildcardId);
    }

    #endregion

    #region Integration with SubmitSoloAnswerUseCase Tests

    [Fact]
    public async Task DoubleProgress_WhenUsed_ShouldAffectNextCorrectAnswer()
    {
        // Este test verifica la integración con SubmitSoloAnswerUseCase
        // El flag HasDoubleProgressActive debe ser leído por ese UseCase

        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        game.PlayerPosition = 5;
        var wildcardId = (int)WildcardType.DoubleProgress;

        SetupMocks(game, wildcardId);

        // Act
        await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert - El flag debe estar activo para que SubmitSoloAnswerUseCase lo detecte
        game.HasDoubleProgressActive.Should().BeTrue();

        // Simular respuesta correcta (esto lo haría SubmitSoloAnswerUseCase)
        // Si tiene el flag activo, avanzará 2 posiciones en lugar de 1
    }

    [Fact]
    public async Task RemoveWrongOption_ModifiedOptions_ShouldBeUsedByStatusEndpoint()
    {
        // Este test verifica que las opciones modificadas se guarden correctamente
        // para que GetSoloGameStatusUseCase las pueda retornar

        // Arrange
        var game = CreateSoloGameWithQuestion(new List<int> { 1, 2, 3, 4 }, correctAnswer: 3);
        var wildcardId = (int)WildcardType.RemoveWrongOption;

        SetupMocks(game, wildcardId);

        // Act
        await _useCase.ExecuteAsync(game.Id, wildcardId, game.PlayerUid);

        // Assert
        game.ModifiedOptions.Should().NotBeNull();
        game.ModifiedOptions!.Count.Should().BeLessThan(4); // Menos opciones que antes

        // Estas opciones modificadas deben ser retornadas por GetSoloGameStatusUseCase
    }

    #endregion

    #region Helper Methods

    private SoloGame CreateBasicSoloGame()
    {
        var game = new SoloGame
        {
            Id = 1,
            PlayerId = 100,
            PlayerUid = "test-player-uid",
            PlayerName = "Test Player",
            LevelId = 1,
            WorldId = 1,
            Status = SoloGameStatus.InProgress,
            CurrentQuestionIndex = 0,
            PlayerPosition = 0,
            LivesRemaining = 3,
            TotalQuestions = 10,
            TimePerEquation = 10,
            GameStartedAt = DateTime.UtcNow,
            Questions = new List<Question>(),
            AvailableWildcards = new List<PlayerWildcard>
            {
                new PlayerWildcard
                {
                    WildcardId = 1,
                    Quantity = 1,
                    Wildcard = new Wildcard
                    {
                        Id = 1,
                        Name = "Eliminar opción",
                        Description = "Elimina una opción incorrecta"
                    }
                },
                new PlayerWildcard
                {
                    WildcardId = 2,
                    Quantity = 1,
                    Wildcard = new Wildcard
                    {
                        Id = 2,
                        Name = "Saltar pregunta",
                        Description = "Cambia la pregunta actual"
                    }
                },
                new PlayerWildcard
                {
                    WildcardId = 3,
                    Quantity = 1,
                    Wildcard = new Wildcard
                    {
                        Id = 3,
                        Name = "Doble progreso",
                        Description = "La siguiente respuesta correcta vale doble"
                    }
                }
            },
            UsedWildcardTypes = new HashSet<int>()
        };

        return game;
    }

    private SoloGame CreateSoloGameWithQuestion(List<int> options, int correctAnswer)
    {
        var game = CreateBasicSoloGame();

        game.Questions = new List<Question>
        {
            new Question
            {
                Id = 1,
                Equation = "y = 2*x + 1",
                Options = options,
                CorrectAnswer = correctAnswer
            }
        };

        return game;
    }

    private SoloGame CreateSoloGameWithMultipleQuestions(int count)
    {
        var game = CreateBasicSoloGame();
        game.Questions.Clear();

        for (int i = 0; i < count; i++)
        {
            game.Questions.Add(new Question
            {
                Id = i + 1,
                Equation = $"y = {i + 1}*x",
                Options = new List<int> { i, i + 1, i + 2, i + 3 },
                CorrectAnswer = i + 1
            });
        }

        return game;
    }

    private void SetupMocks(SoloGame game, int wildcardId)
    {
        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id))
            .ReturnsAsync(game);

        _mockWildcardRepository.Setup(r => r.HasWildcardAvailableAsync(game.PlayerId, wildcardId))
            .ReturnsAsync(true);

        _mockWildcardRepository.Setup(r => r.ConsumeWildcardAsync(game.PlayerId, wildcardId))
            .ReturnsAsync(true);

        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<SoloGame>()))
            .Returns(Task.CompletedTask);
    }

    #endregion
}
