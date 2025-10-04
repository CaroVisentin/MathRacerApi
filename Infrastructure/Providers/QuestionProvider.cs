using System.Text.Json;
using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Infrastructure.Providers;

public class QuestionProvider
{
    private readonly string _filePath;

    public QuestionProvider(string filePath)
    {
        _filePath = filePath;
    }

    public List<JsonQuestion> GetQuestions()
    {
        var json = File.ReadAllText(_filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var questions = JsonSerializer.Deserialize<List<JsonQuestion>>(json, options);
       
        return questions ?? new List<JsonQuestion>();
    }
}