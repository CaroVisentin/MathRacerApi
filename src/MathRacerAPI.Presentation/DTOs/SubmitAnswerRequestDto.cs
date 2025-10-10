using System;

namespace MathRacerAPI.Presentation.DTOs;

public class SubmitAnswerRequestDto
{
    public int PlayerId { get; set; }
    public int Answer { get; set; }
}