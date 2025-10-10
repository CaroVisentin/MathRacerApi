using System;
using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs;

public class QuestionResponseDto
{
    public int QuestionId { get; set; }
    public string Equation { get; set; } = string.Empty;
    public List<int> Options { get; set; } = new();
}