using System;
using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// DTO de respuesta para el inicio de una partida individual
/// </summary>
public class StartSoloGameResponseDto
{
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int LevelId { get; set; }
    public int TotalQuestions { get; set; }
    public int TimePerEquation { get; set; }
    public int LivesRemaining { get; set; }
    public string ResultType { get; set; } = string.Empty;
    public DateTime GameStartedAt { get; set; }
    public SoloQuestionDto? CurrentQuestion { get; set; }
    public List<SoloProductDto> PlayerProducts { get; set; } = new();
    public List<SoloProductDto> MachineProducts { get; set; } = new();
    public List<WildcardDto> AvailableWildcards { get; set; } = new();
}

/// <summary>
/// DTO para representar una pregunta en modo individual
/// </summary>
public class SoloQuestionDto
{
    public int Id { get; set; }
    public string Equation { get; set; } = string.Empty;
    public List<int> Options { get; set; } = new();
    public DateTime StartedAt { get; set; }
}

/// <summary>
/// DTO para representar un producto en modo individual (auto, personaje, fondo)
/// </summary>
public class SoloProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProductTypeId { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public int RarityId { get; set; }
    public string RarityName { get; set; } = string.Empty;
    public string RarityColor { get; set; } = string.Empty;
}