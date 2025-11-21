using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using Moq;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de obtención de wildcards del jugador
/// </summary>
public class GetPlayerWildcardsUseCaseTests
{
    private readonly Mock<IWildcardRepository> _mockWildcardRepository;
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly GetPlayerWildcardsUseCase _useCase;

    public GetPlayerWildcardsUseCaseTests()
    {
        _mockWildcardRepository = new Mock<IWildcardRepository>();
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _useCase = new GetPlayerWildcardsUseCase(_mockWildcardRepository.Object, _mockPlayerRepository.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenWildcardRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new GetPlayerWildcardsUseCase(null!, _mockPlayerRepository.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("wildcardRepository");
    }

    [Fact]
    public void Constructor_WhenPlayerRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new GetPlayerWildcardsUseCase(_mockWildcardRepository.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("playerRepository");
    }

    #endregion

    #region ExecuteByUidAsync Tests

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerExists_ShouldReturnWildcardsList()
    {
        // Arrange
        var uid = "test-uid-123";
        var playerId = 1;
        var player = new PlayerProfile { Id = playerId, Uid = uid, Name = "Test Player" };

        var wildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard
            {
                PlayerId = playerId,
                WildcardId = 2,
                Quantity = 3,
                Wildcard = new Wildcard { Id = 2, Name = "Matafuego", Description = "Elimina una opción" }
            },
            new PlayerWildcard
            {
                PlayerId = playerId,
                WildcardId = 1,
                Quantity = 1,
                Wildcard = new Wildcard { Id = 1, Name = "Reloj", Description = "Congela el tiempo" }
            }
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ReturnsAsync(player);
        _mockWildcardRepository.Setup(r => r.GetPlayerWildcardsAsync(playerId))
            .ReturnsAsync(wildcards);

        // Act
        var result = await _useCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(w => w.WildcardId);
        _mockPlayerRepository.Verify(r => r.GetByUidAsync(uid), Times.Once);
        _mockWildcardRepository.Verify(r => r.GetPlayerWildcardsAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var uid = "nonexistent-uid";
        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteByUidAsync(uid);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Jugador con UID '{uid}' no encontrado.");
        _mockWildcardRepository.Verify(r => r.GetPlayerWildcardsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerHasNoWildcards_ShouldReturnEmptyList()
    {
        // Arrange
        var uid = "test-uid-456";
        var playerId = 2;
        var player = new PlayerProfile { Id = playerId, Uid = uid, Name = "Player Without Wildcards" };
        var emptyWildcards = new List<PlayerWildcard>();

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ReturnsAsync(player);
        _mockWildcardRepository.Setup(r => r.GetPlayerWildcardsAsync(playerId))
            .ReturnsAsync(emptyWildcards);

        // Act
        var result = await _useCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockPlayerRepository.Verify(r => r.GetByUidAsync(uid), Times.Once);
        _mockWildcardRepository.Verify(r => r.GetPlayerWildcardsAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByUidAsync_ShouldReturnWildcardsOrderedByWildcardId()
    {
        // Arrange
        var uid = "test-uid-789";
        var playerId = 3;
        var player = new PlayerProfile { Id = playerId, Uid = uid, Name = "Test Player" };

        var unorderedWildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard
            {
                PlayerId = playerId,
                WildcardId = 3,
                Quantity = 2,
                Wildcard = new Wildcard { Id = 3, Name = "Wildcard C", Description = "Description C" }
            },
            new PlayerWildcard
            {
                PlayerId = playerId,
                WildcardId = 1,
                Quantity = 5,
                Wildcard = new Wildcard { Id = 1, Name = "Wildcard A", Description = "Description A" }
            },
            new PlayerWildcard
            {
                PlayerId = playerId,
                WildcardId = 2,
                Quantity = 3,
                Wildcard = new Wildcard { Id = 2, Name = "Wildcard B", Description = "Description B" }
            }
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ReturnsAsync(player);
        _mockWildcardRepository.Setup(r => r.GetPlayerWildcardsAsync(playerId))
            .ReturnsAsync(unorderedWildcards);

        // Act
        var result = await _useCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(w => w.WildcardId);
        result[0].WildcardId.Should().Be(1);
        result[1].WildcardId.Should().Be(2);
        result[2].WildcardId.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerHasMultipleWildcards_ShouldReturnAllWithCorrectData()
    {
        // Arrange
        var uid = "test-uid-complete";
        var playerId = 4;
        var player = new PlayerProfile { Id = playerId, Uid = uid, Name = "Complete Player" };

        var wildcards = new List<PlayerWildcard>
        {
            new PlayerWildcard
            {
                PlayerId = playerId,
                WildcardId = 1,
                Quantity = 3,
                Wildcard = new Wildcard { Id = 1, Name = "Matafuego", Description = "Elimina una opción incorrecta" }
            },
            new PlayerWildcard
            {
                PlayerId = playerId,
                WildcardId = 2,
                Quantity = 1,
                Wildcard = new Wildcard { Id = 2, Name = "Reloj de Arena", Description = "Congela el tiempo" }
            },
            new PlayerWildcard
            {
                PlayerId = playerId,
                WildcardId = 3,
                Quantity = 5,
                Wildcard = new Wildcard { Id = 3, Name = "Bomba", Description = "Duplica puntos" }
            }
        };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ReturnsAsync(player);
        _mockWildcardRepository.Setup(r => r.GetPlayerWildcardsAsync(playerId))
            .ReturnsAsync(wildcards);

        // Act
        var result = await _useCase.ExecuteByUidAsync(uid);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(w => w.WildcardId);

        // Verificar primer wildcard
        result[0].WildcardId.Should().Be(1);
        result[0].Quantity.Should().Be(3);
        result[0].Wildcard.Name.Should().Be("Matafuego");

        // Verificar segundo wildcard
        result[1].WildcardId.Should().Be(2);
        result[1].Quantity.Should().Be(1);
        result[1].Wildcard.Name.Should().Be("Reloj de Arena");

        // Verificar tercer wildcard
        result[2].WildcardId.Should().Be(3);
        result[2].Quantity.Should().Be(5);
        result[2].Wildcard.Name.Should().Be("Bomba");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ExecuteByUidAsync_WhenUidIsInvalid_ShouldStillAttemptToFindPlayer(string invalidUid)
    {
        // Arrange
        _mockPlayerRepository.Setup(r => r.GetByUidAsync(invalidUid))
            .ReturnsAsync((PlayerProfile?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteByUidAsync(invalidUid);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _mockPlayerRepository.Verify(r => r.GetByUidAsync(invalidUid), Times.Once);
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenWildcardRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var uid = "test-uid-exception";
        var playerId = 5;
        var player = new PlayerProfile { Id = playerId, Uid = uid, Name = "Exception Player" };

        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ReturnsAsync(player);
        _mockWildcardRepository.Setup(r => r.GetPlayerWildcardsAsync(playerId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _useCase.ExecuteByUidAsync(uid);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task ExecuteByUidAsync_WhenPlayerRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var uid = "test-uid-exception-2";
        _mockPlayerRepository.Setup(r => r.GetByUidAsync(uid))
            .ThrowsAsync(new Exception("Player repository error"));

        // Act
        Func<Task> act = async () => await _useCase.ExecuteByUidAsync(uid);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Player repository error");
        _mockWildcardRepository.Verify(r => r.GetPlayerWildcardsAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}
