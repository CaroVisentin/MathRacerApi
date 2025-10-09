using System;

namespace MathRacerAPI.Presentation.DTOs;

public class SubmitAnswerRequestDto
{
    public int PlayerId { get; set; }
    public string Answer { get; set; } = string.Empty;
}