using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para crear una partida multijugador personalizada
/// </summary>
public class CreateCustomOnlineGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IWorldRepository _worldRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;
    private readonly IPowerUpService _powerUpService;
    private readonly IPlayerRepository _playerRepository;
    private static int _nextGameId = 1000;

    public CreateCustomOnlineGameUseCase(
        IGameRepository gameRepository,
        IWorldRepository worldRepository,
        ILevelRepository levelRepository,
        GetQuestionsUseCase getQuestionsUseCase,
        IPowerUpService powerUpService,
        IPlayerRepository playerRepository)
    {
        _gameRepository = gameRepository;
        _worldRepository = worldRepository;
        _levelRepository = levelRepository;
        _getQuestionsUseCase = getQuestionsUseCase;
        _powerUpService = powerUpService;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Crea una partida personalizada SIN agregar jugadores aún.
    /// El creador se unirá posteriormente mediante JoinCreatedGameUseCase con su ConnectionId real.
    /// </summary>
    public async Task<Game> ExecuteAsync(
        string firebaseUid,
        string gameName,
        bool isPrivate,
        string? password,
        string difficulty,
        string expectedResult)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(gameName))
            throw new ValidationException("El nombre de la partida es requerido");

        if (isPrivate && string.IsNullOrWhiteSpace(password))
            throw new ValidationException("La contraseña es requerida para partidas privadas");

        // Obtener perfil del creador
        var creatorProfile = await _playerRepository.GetByUidAsync(firebaseUid);
        if (creatorProfile == null)
            throw new NotFoundException("Perfil de jugador no encontrado");

        // Determinar parámetros según dificultad
        var (termCount, variableCount, operations, numMin, numMax, optMin, optMax, timePerEq) =
            GetDifficultyParameters(difficulty);

        // Crear partida SIN jugadores
        var game = new Game
        {
            Id = Interlocked.Increment(ref _nextGameId),
            Name = gameName,
            IsPrivate = isPrivate,
            Password = isPrivate ? password : null,
            Status = GameStatus.WaitingForPlayers, // ✅ Esperando jugadores
            CreatedAt = DateTime.UtcNow,
            PowerUpsEnabled = true,
            MaxPowerUpsPerPlayer = 3,
            MaxQuestions = 10,
            ConditionToWin = 5,
            ExpectedResult = expectedResult,
            CreatorPlayerId = null, // ⚠️ Se asignará cuando el creador se una con JoinGame
            Players = new List<Player>() // ✅ Lista vacía inicialmente
        };

        // Generar preguntas
        var equationParams = new EquationParams
        {
            TermCount = termCount,
            VariableCount = variableCount,
            Operations = operations,
            ExpectedResult = expectedResult,
            OptionsCount = 4,
            OptionRangeMin = optMin,
            OptionRangeMax = optMax,
            NumberRangeMin = numMin,
            NumberRangeMax = numMax,
            TimePerEquation = timePerEq
        };

        var questions = await _getQuestionsUseCase.GetQuestions(equationParams, game.MaxQuestions);
        game.Questions = questions;

        await _gameRepository.AddAsync(game);
        return game;
    }

    private static (int termCount, int variableCount, List<string> operations,
                    int numMin, int numMax, int optMin, int optMax, int timePerEq)
        GetDifficultyParameters(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "facil" => (2, 1, new List<string> { "+", "-" }, -10, 10, -10, 10, 15),
            "medio" => (3, 1, new List<string> { "+", "-", "*" }, -20, 20, -20, 20, 12),
            "dificil" => (4, 1, new List<string> { "+", "-", "*", "/" }, -50, 50, -50, 50, 10),
            _ => (2, 1, new List<string> { "+", "-" }, -10, 10, -10, 10, 15)
        };
    }
}