using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Solo;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controlador para el modo de juego individual
/// </summary>
[ApiController]
[Route("api/solo")]
public class SoloController : ControllerBase
{
    private readonly StartSoloGameUseCase _startSoloGameUseCase;
    private readonly GetSoloGameStatusUseCase _getSoloGameStatusUseCase;
    private readonly SubmitSoloAnswerUseCase _submitSoloAnswerUseCase;
    private readonly IFirebaseService _firebaseService;

    public SoloController(
        StartSoloGameUseCase startSoloGameUseCase,
        GetSoloGameStatusUseCase getSoloGameStatusUseCase,
        SubmitSoloAnswerUseCase submitSoloAnswerUseCase,
        IFirebaseService firebaseService)
    {
        _startSoloGameUseCase = startSoloGameUseCase;
        _getSoloGameStatusUseCase = getSoloGameStatusUseCase;
        _submitSoloAnswerUseCase = submitSoloAnswerUseCase;
        _firebaseService = firebaseService;
    }

    /// <summary>
    /// Inicia una nueva partida individual
    /// </summary>
    /// <param name="levelId">ID del nivel a jugar</param>
    /// <returns>Información inicial de la partida</returns>
    /// <response code="200">Partida iniciada exitosamente</response>
    /// <response code="400">No tienes energía suficiente</response>
    /// <response code="401">Token inválido</response>
    /// <response code="404">Nivel no encontrado</response>
    [HttpPost("start/{levelId}")]
    [ProducesResponseType(typeof(StartSoloGameResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StartSoloGameResponseDto>> StartGame(int levelId)
    {
        // 1. Validar token y obtener UID
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        // 2. Iniciar partida
        var game = await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        // 3. Mapear respuesta
        var currentQuestion = game.Questions.FirstOrDefault();
        var response = new StartSoloGameResponseDto
        {
            GameId = game.Id,
            PlayerId = game.PlayerId,
            PlayerName = game.PlayerName,
            LevelId = game.LevelId,
            TotalQuestions = game.TotalQuestions,
            TimePerEquation = game.TimePerEquation,
            LivesRemaining = game.LivesRemaining,
            GameStartedAt = game.GameStartedAt,
            CurrentQuestion = currentQuestion != null ? new QuestionDto
            {
                Id = currentQuestion.Id,
                Equation = currentQuestion.Equation,
                Options = currentQuestion.Options,
                StartedAt = game.CurrentQuestionStartedAt ?? DateTime.UtcNow
            } : new QuestionDto()
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtiene el estado actual de una partida individual
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <returns>Estado completo de la partida</returns>
    /// <response code="200">Estado obtenido exitosamente</response>
    /// <response code="401">Token inválido o no autorizado</response>
    /// <response code="403">No tienes permiso para acceder a esta partida</response>
    /// <response code="404">Partida no encontrada</response>
    [HttpGet("{gameId}")]
    [ProducesResponseType(typeof(SoloGameStatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoloGameStatusResponseDto>> GetGameStatus(int gameId)
    {
        // 1. Validar token y obtener UID
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        // 2. Obtener estado de la partida con validación de permisos
        var game = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        // 3. Calcular tiempo restante para la pregunta actual
        var remainingTime = 0.0;
        if (game.CurrentQuestionStartedAt.HasValue && game.Status == Domain.Models.SoloGameStatus.InProgress)
        {
            var elapsed = (DateTime.UtcNow - game.CurrentQuestionStartedAt.Value).TotalSeconds;
            remainingTime = Math.Max(0, game.TimePerEquation - elapsed);
        }

        // 4. Obtener pregunta actual
        QuestionDto? currentQuestion = null;
        if (game.CurrentQuestionIndex < game.Questions.Count)
        {
            var question = game.Questions[game.CurrentQuestionIndex];
            currentQuestion = new QuestionDto
            {
                Id = question.Id,
                Equation = question.Equation,
                Options = question.Options,
                StartedAt = game.CurrentQuestionStartedAt ?? DateTime.UtcNow
            };
        }

        var response = new SoloGameStatusResponseDto
        {
            GameId = game.Id,
            Status = game.Status.ToString(),
            PlayerPosition = game.PlayerPosition,
            LivesRemaining = game.LivesRemaining,
            CorrectAnswers = game.CorrectAnswers,
            MachinePosition = game.MachinePosition,
            CurrentQuestion = currentQuestion,
            CurrentQuestionIndex = game.CurrentQuestionIndex,
            TotalQuestions = game.TotalQuestions,
            TimePerEquation = game.TimePerEquation,
            WinnerId = game.WinnerId,
            WinnerName = game.WinnerId == game.PlayerId ? game.PlayerName : 
                        game.WinnerId == -1 ? "Máquina" : null,
            GameStartedAt = game.GameStartedAt,
            GameFinishedAt = game.GameFinishedAt,
            ElapsedTime = (DateTime.UtcNow - game.GameStartedAt).TotalSeconds,
            RemainingTimeForQuestion = remainingTime
        };

        return Ok(response);
    }

    /// <summary>
    /// Envía una respuesta a la pregunta actual
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <param name="answer">Respuesta del jugador (en el body como JSON)</param>
    /// <returns>Feedback minimalista de la respuesta</returns>
    /// <response code="200">Respuesta procesada exitosamente</response>
    /// <response code="400">Error al procesar respuesta</response>
    /// <response code="401">Token inválido</response>
    /// <response code="403">No autorizado para esta partida</response>
    /// <response code="404">Partida no encontrada</response>
    [HttpPost("{gameId}/answer")]
    [ProducesResponseType(typeof(SubmitSoloAnswerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmitSoloAnswerResponseDto>> SubmitAnswer(
        int gameId, 
        [FromBody] int answer)  // ✅ Directamente el valor, sin DTO wrapper
    {
        // 1. Validar token y obtener UID
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        // 2. Procesar respuesta
        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);
        var game = result.Game;

        // 3. Crear respuesta minimalista con solo lo esencial
        var response = new SubmitSoloAnswerResponseDto
        {
            // Feedback de la respuesta
            IsCorrect = result.IsCorrect,
            CorrectAnswer = result.CorrectAnswer,
            PlayerAnswer = result.PlayerAnswer,
            
            // Estado crítico del juego
            Status = game.Status.ToString(),
            LivesRemaining = game.LivesRemaining,
            PlayerPosition = game.PlayerPosition,
            TotalQuestions = game.TotalQuestions,
            
            // Información del ganador (solo si el juego terminó)
            WinnerId = game.WinnerId,
            WinnerName = game.WinnerId == game.PlayerId ? game.PlayerName : 
                        game.WinnerId == -1 ? "Máquina" : null
        };

        return Ok(response);
    }
}