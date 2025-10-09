using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Providers;
using System.Text.Json;

namespace MathRacerAPI.Domain.UseCases;

public class CreateGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IQuestionProvider _questionProvider;

    public CreateGameUseCase(IGameRepository gameRepository, IQuestionProvider questionProvider)
    {
        _gameRepository = gameRepository;
        _questionProvider = questionProvider;
    }

    public async Task<Game> ExecuteAsync(string playerName)
    {
        var player = new Player { Name = playerName, Id = 1};  //Creo el jugador que crea la partida
        var game = new Game();
        game.Id = 1;    //Asigno un id a la partida
        game.Players.Add(player);  //Agrego el jugador a la partida

        var types = new[] { "mayor", "menor", "igual" }; //Tipos de preguntas
        var random = new Random();
        var typeSelected = types[random.Next(types.Length)]; //Obtengo de manera aleatoria el tipo de resultado esperado

        var allQuestions = _questionProvider.GetQuestions();  //Obtengo todas las preguntas del proveedor

        var selected = allQuestions
            .Where(q => q.Result == typeSelected)    
            .OrderBy(q => q.Equation)   
            .Take(game.MaxQuestions)
            .ToList();

        game.Questions = selected.Select(q => new Question    //Mapeo las preguntas al modelo de dominio
        {
            Id = q.Id,
            Equation = q.Equation,
            Options = q.Options.Select(o => o.Value.ToString()).ToList(),
            CorrectAnswer = q.Options.First(o => o.IsCorrect).Value.ToString()
        }).ToList();

        await _gameRepository.AddAsync(game);
        return game;
    }
}