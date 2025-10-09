using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/games")]
[Tags("Game")]
public class GameController : ControllerBase
{
    private readonly CreateGameUseCase _createGameUseCase;
    private readonly JoinGameUseCase _joinGameUseCase;
    private readonly GetNextQuestionUseCase _getNextQuestionUseCase;
    private readonly SubmitAnswerUseCase _submitAnswerUseCase;

    public GameController(
        CreateGameUseCase createGameUseCase,
        JoinGameUseCase joinGameUseCase,
        GetNextQuestionUseCase getNextQuestionUseCase,
        SubmitAnswerUseCase submitAnswerUseCase)
    {
        _createGameUseCase = createGameUseCase; //Instancio los casos de uso
        _joinGameUseCase = joinGameUseCase;
        _getNextQuestionUseCase = getNextQuestionUseCase;
        _submitAnswerUseCase = submitAnswerUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<GameResponseDto>> CreateGame([FromBody] CreateGameRequestDto request)
    {
        var game = await _createGameUseCase.ExecuteAsync(request.PlayerName); //Ejecuto el caso de uso para crear la partida

        var winnerName = game.WinnerId.HasValue
            ? game.Players.FirstOrDefault(p => p.Id == game.WinnerId.Value)?.Name
            : null;

        var response = new GameResponseDto //Mapeo para mostrar en el front
        {
            GameId = game.Id,
            Players = game.Players.Select(p => new PlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                CorrectAnswers = p.CorrectAnswers,
                IndexAnswered = p.IndexAnswered,
                Position = p.Position
            }).ToList(),
            Status = game.Status.ToString(),
            MaxQuestions = game.MaxQuestions,
            WinnerId = game.WinnerId,
            WinnerName = winnerName 
        };

        return Ok(response); //Devuelvo la respuesta
    }

    [HttpPost("{id}/join")]
    public async Task<ActionResult<GameResponseDto>> JoinGame(int id, [FromBody] JoinGameRequestDto request)
    {
        var game = await _joinGameUseCase.ExecuteAsync(id, request.PlayerName);
        if (game == null) return NotFound();

        var winnerName = game.WinnerId.HasValue
            ? game.Players.FirstOrDefault(p => p.Id == game.WinnerId.Value)?.Name
            : null;

        var response = new GameResponseDto //Mapeo para mostrar en el front
        {
            GameId = game.Id,
            Players = game.Players.Select(p => new PlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                CorrectAnswers = p.CorrectAnswers,
                IndexAnswered = p.IndexAnswered,
                Position = p.Position
            }).ToList(),
            Status = game.Status.ToString(),
            MaxQuestions = game.MaxQuestions,
            WinnerId = game.WinnerId,
            WinnerName = winnerName 
        };

        return Ok(response);
    }

    [HttpGet("{id}/question")]
    public async Task<ActionResult<QuestionResponseDto>> GetNextQuestion(int id, [FromQuery] int playerId)
    {
        var result = await _getNextQuestionUseCase.ExecuteAsync(id, playerId);

        if (result.PenaltySecondsLeft.HasValue) //Informo al jugador que está penalizado
        {
            return StatusCode(429, new
            {
                message = "Penalizado. Esperá antes de la próxima pregunta.",
                secondsLeft = result.PenaltySecondsLeft
            });
        }

        if (result.Question == null)
            return NotFound();

        var response = new QuestionResponseDto //Mapeo para mostrar en el front
        {
            QuestionId = result.Question.Id,
            Equation = result.Question.Equation,
            Options = result.Question.Options
        };

        return Ok(response);
    }

    [HttpPost("{id}/answer")]
    public async Task<ActionResult<GameResponseDto>> SubmitAnswer(int id, [FromBody] SubmitAnswerRequestDto request)
    {
        var game = await _submitAnswerUseCase.ExecuteAsync(id, request.PlayerId, request.Answer);
        if (game == null) return NotFound();

        var response = new GameResponseDto
        {
            GameId = game.Id,
            Players = game.Players.Select(p => new PlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                CorrectAnswers = p.CorrectAnswers,
                IndexAnswered = p.IndexAnswered,
                Position = p.Position
            }).ToList(),
            Status = game.Status.ToString(),
            MaxQuestions = game.MaxQuestions,
            WinnerId = game.WinnerId,
            WinnerName = game.WinnerId.HasValue
                ? game.Players.FirstOrDefault(p => p.Id == game.WinnerId.Value)?.Name
                : null
        };

        return Ok(response);
    }

}