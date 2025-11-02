using System;
using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs.Solo;

/// <summary>
/// DTO de respuesta para el inicio de una partida individual
/// </summary>
public class StartSoloGameResponseDto
{
    /// <summary>
    /// ID único de la partida generada
    /// </summary>
    public int GameId { get; set; }

    /// <summary>
    /// ID del jugador que inició la partida
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// Nombre del jugador
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// ID del nivel seleccionado
    /// </summary>
    public int LevelId { get; set; }

    /// <summary>
    /// Cantidad total de preguntas en la partida
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Tiempo en segundos asignado por ecuación
    /// </summary>
    public int TimePerEquation { get; set; }

    /// <summary>
    /// Vidas restantes del jugador (inicial: 3)
    /// </summary>
    public int LivesRemaining { get; set; }

    /// <summary>
    /// Fecha y hora UTC de inicio de la partida
    /// </summary>
    public DateTime GameStartedAt { get; set; }

    /// <summary>
    /// Primera pregunta de la partida con sus opciones de respuesta
    /// </summary>
    public SoloQuestionDto? CurrentQuestion { get; set; }

    /// <summary>
    /// Lista de productos activos del jugador (auto, personaje, fondo)
    /// </summary>
    public List<SoloProductDto> PlayerProducts { get; set; } = new();

    /// <summary>
    /// Lista de productos aleatorios asignados a la máquina
    /// </summary>
    public List<SoloProductDto> MachineProducts { get; set; } = new();
}

/// <summary>
/// DTO para representar una pregunta en modo individual
/// </summary>
public class SoloQuestionDto
{
    /// <summary>
    /// ID único de la pregunta
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ecuación matemática a resolver (ej: "y = 2*x + 3")
    /// </summary>
    public string Equation { get; set; } = string.Empty;

    /// <summary>
    /// Opciones de respuesta disponibles para el jugador
    /// </summary>
    public List<int> Options { get; set; } = new();

    /// <summary>
    /// Fecha y hora UTC en que se mostró la pregunta al jugador
    /// </summary>
    public DateTime StartedAt { get; set; }
}

/// <summary>
/// DTO para representar un producto en modo individual (auto, personaje, fondo)
/// </summary>
public class SoloProductDto
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ID del tipo de producto (1: Auto, 2: Personaje, 3: Fondo)
    /// </summary>
    public int ProductTypeId { get; set; }

    /// <summary>
    /// Nombre del tipo de producto
    /// </summary>
    public string ProductTypeName { get; set; } = string.Empty;

    /// <summary>
    /// ID de la rareza del producto
    /// </summary>
    public int RarityId { get; set; }

    /// <summary>
    /// Nombre de la rareza (Común, Poco común, Raro, etc.)
    /// </summary>
    public string RarityName { get; set; } = string.Empty;

    /// <summary>
    /// Color hexadecimal asociado a la rareza (ej: "#FFFFFF")
    /// </summary>
    public string RarityColor { get; set; } = string.Empty;
}