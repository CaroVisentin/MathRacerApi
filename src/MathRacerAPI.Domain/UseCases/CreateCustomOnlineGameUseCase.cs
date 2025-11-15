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

    public async Task<Game> ExecuteAsync(
        string firebaseUid,
        string gameName,
        string connectionId,
        bool isPrivate,
        string? password,
        string difficulty,
        string expectedResult)
    {
        // Validar que si es privada tenga contraseña
        if (isPrivate && string.IsNullOrWhiteSpace(password))
        {
            throw new BusinessException("Las partidas privadas requieren una contraseña.");
        }

        // Validar nombre de partida
        if (string.IsNullOrWhiteSpace(gameName))
        {
            throw new BusinessException("El nombre de la partida es requerido.");
        }

        // Obtener el perfil del jugador desde la base de datos usando el UID de Firebase
        var playerProfile = await _playerRepository.GetByUidAsync(firebaseUid);
        if (playerProfile == null)
        {
            throw new NotFoundException("Player", firebaseUid);
        }

        // Verificar que el jugador no tenga otra partida activa
        var allGames = await _gameRepository.GetAllAsync();
        var activeGame = allGames.FirstOrDefault(g =>
            g.Players.Any(p => p.Id == playerProfile.Id) &&
            (g.Status == GameStatus.WaitingForPlayers || g.Status == GameStatus.InProgress));

        if (activeGame != null)
        {
            throw new BusinessException(
                $"Ya tienes una partida activa (ID: {activeGame.Id}). " +
                $"Debes finalizarla o abandonarla antes de crear una nueva."
            );
        }

        // Normalizar dificultad y resultado esperado
        difficulty = difficulty.ToUpperInvariant();
        expectedResult = expectedResult.ToUpperInvariant();

        // Validar dificultad
        if (difficulty != "FACIL" && difficulty != "MEDIO" && difficulty != "DIFICIL")
        {
            throw new BusinessException("La dificultad debe ser FACIL, MEDIO o DIFICIL.");
        }

        // Validar resultado esperado
        if (expectedResult != "MAYOR" && expectedResult != "MENOR")
        {
            throw new BusinessException("El resultado esperado debe ser MAYOR o MENOR.");
        }

        // Obtener todos los mundos
        var allWorlds = await _worldRepository.GetAllWorldsAsync();

        // Filtrar mundos por dificultad (comparación case-insensitive)
        var worldsWithDifficulty = allWorlds
            .Where(w => w.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!worldsWithDifficulty.Any())
        {
            throw new BusinessException($"No se encontraron mundos con dificultad {difficulty}.");
        }

        // Seleccionar un mundo aleatorio de los que coinciden con la dificultad
        var random = new Random();
        var selectedWorld = worldsWithDifficulty[random.Next(worldsWithDifficulty.Count)];

        // Obtener todos los niveles del mundo seleccionado
        var levelsInWorld = await _levelRepository.GetAllByWorldIdAsync(selectedWorld.Id);

        if (!levelsInWorld.Any())
        {
            throw new BusinessException($"El mundo {selectedWorld.Name} no tiene niveles configurados.");
        }

        // Seleccionar nivel según dificultad
        // FACIL = nivel 1, MEDIO = nivel 6, DIFICIL = nivel 11
        int levelNumber = difficulty switch
        {
            "FACIL" => 1,
            "MEDIO" => 6,
            "DIFICIL" => 11,
            _ => 1
        };

        var selectedLevel = levelsInWorld.FirstOrDefault(l => l.Number == levelNumber);

        if (selectedLevel == null)
        {
            throw new BusinessException($"No se encontró el nivel {levelNumber} en el mundo {selectedWorld.Name}.");
        }

        // Crear jugador para la partida usando el perfil existente
        var player = new Player
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            ConnectionId = connectionId,
            LastLevelId = playerProfile.LastLevelId ?? 0
        };

        // Otorgar power-ups iniciales
        player.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(player.Id);

        // Crear nueva partida
        var game = new Game
        {
            Id = Interlocked.Increment(ref _nextGameId),
            Name = gameName,
            IsPrivate = isPrivate,
            Password = password,
            Status = GameStatus.WaitingForPlayers,
            CreatedAt = DateTime.UtcNow,
            PowerUpsEnabled = true,
            MaxPowerUpsPerPlayer = 3,
            ExpectedResult = expectedResult,
            CreatorPlayerId = playerProfile.Id
        };

        game.Players.Add(player);

        // Construir EquationParams usando datos del mundo y nivel seleccionados
        var equationParams = new EquationParams
        {
            TermCount = selectedLevel.TermsCount,
            VariableCount = selectedLevel.VariablesCount,
            Operations = selectedWorld.Operations,
            ExpectedResult = expectedResult,
            OptionsCount = selectedWorld.OptionsCount,
            OptionRangeMin = selectedWorld.OptionRangeMin,
            OptionRangeMax = selectedWorld.OptionRangeMax,
            NumberRangeMin = selectedWorld.NumberRangeMin,
            NumberRangeMax = selectedWorld.NumberRangeMax,
            TimePerEquation = selectedWorld.TimePerEquation
        };

        // Generar preguntas
        var allQuestions = await _getQuestionsUseCase.GetQuestions(equationParams, game.MaxQuestions);
        game.Questions = allQuestions;

        await _gameRepository.AddAsync(game);
        return game;
    }
}