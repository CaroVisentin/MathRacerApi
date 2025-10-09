using System.Text.Json;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Providers;

namespace MathRacerAPI.Infrastructure.Providers;

public class QuestionProvider : IQuestionProvider
{
    private readonly string _filePath;

    public QuestionProvider(string filePath)
    {
        _filePath = filePath;
    }

    public List<JsonQuestion> GetQuestions()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"No se encontró el archivo de preguntas en: {_filePath}");
            }

            var json = File.ReadAllText(_filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var questions = JsonSerializer.Deserialize<List<JsonQuestion>>(json, options);
           
            return questions ?? new List<JsonQuestion>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al cargar preguntas desde {_filePath}: {ex.Message}", ex);
        }
    }

    public Question GetRandomQuestion()
    {
        var questions = GetQuestions();
        if (!questions.Any())
        {
            throw new InvalidOperationException("No hay preguntas disponibles");
        }

        var random = new Random();
        var jsonQuestion = questions[random.Next(questions.Count)];

        // Convertir JsonQuestion a Question
        var question = new Question
        {
            Id = jsonQuestion.Id,
            Equation = jsonQuestion.Equation,
            CorrectAnswer = jsonQuestion.Result, // Usar Result como CorrectAnswer
            Options = jsonQuestion.Options.Select(opt => opt.Value.ToString()).ToList() // Convertir int a string
        };

        return question;
    }
}