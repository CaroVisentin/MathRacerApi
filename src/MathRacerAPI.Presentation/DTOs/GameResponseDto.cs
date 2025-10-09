using System;
using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs;

public class GameResponseDto
{
    public int GameId { get; set; }
    public List<PlayerDto> Players { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public int MaxQuestions { get; set; }
    public int? WinnerId { get; set; }
    public string? WinnerName { get; set; }
}

public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CorrectAnswers { get; set; }
    public int IndexAnswered { get; internal set; }
    public int Position { get; set; }

}