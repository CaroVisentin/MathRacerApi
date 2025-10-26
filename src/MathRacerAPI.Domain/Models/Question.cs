using System;
using System.Collections.Generic;

namespace MathRacerAPI.Domain.Models;

public class Question
{
    public int Id { get; set; }
    public string Equation { get; set; } = string.Empty;
    public List<int> Options { get; set; } = new();
    public int CorrectAnswer { get; set; }
}