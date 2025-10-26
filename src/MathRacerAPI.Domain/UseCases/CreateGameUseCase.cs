using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Text.Json;

namespace MathRacerAPI.Domain.UseCases;

public class CreateGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly GetQuestionsUseCase _getQuestionsUseCase;

    public CreateGameUseCase(IGameRepository gameRepository, GetQuestionsUseCase getQuestionsUseCase)
    {
        _gameRepository = gameRepository;
        _getQuestionsUseCase = getQuestionsUseCase;
    }

    public async Task<Game> ExecuteAsync(string playerName)
    {
        var player = new Player { Name = playerName, Id = 1 };  //Creo el jugador que crea la partida 
        var game = new Game();
        game.Id = 1;    //Asigno un id a la partida
        game.Players.Add(player);  //Agrego el jugador a la partida

        var random = new Random();
        var equationParams = new EquationParams
        {
            TermCount = 2,
            VariableCount = 1,
            Operations = new List<string> { "+", "-" },
            ExpectedResult = random.Next(0, 2) == 0 ? "MAYOR" : "MENOR",
            OptionsCount = 4,
            OptionRangeMin = -10,
            OptionRangeMax = 10,
            NumberRangeMin = -10,
            NumberRangeMax = 10,
            TimePerEquation = 10
        };     

        var allQuestions = await _getQuestionsUseCase.GetQuestions(equationParams, game.MaxQuestions);
        game.Questions = allQuestions;
        game.ExpectedResult = equationParams.ExpectedResult;

        await _gameRepository.AddAsync(game);
        return game;
    }
}