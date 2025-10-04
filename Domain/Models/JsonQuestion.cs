using System.Collections.Generic;

namespace MathRacerAPI.Domain.Models;

public class JsonQuestion
{
    public int Id { get; set; }
    public string Equation { get; set; } = string.Empty;
    public List<JsonOption> Options { get; set; } = new();
    public string Result { get; set; } = string.Empty;
    
}

public class JsonOption
{
    public int Value { get; set; }
    public bool IsCorrect { get; set; }
}