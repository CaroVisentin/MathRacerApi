using MathRacerAPI.Domain.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MathRacerAPI.Presentation.DTOs;

/// <summary>
/// DTO para el perfil del jugador
/// </summary>
public class PlayerProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int LastLevelId { get; set; }
    public int Points { get; set; }
    public int Coins { get; set; }
    public EnergyStatusDto? EnergyStatus { get; set; }
    public ActiveProductDto? Car { get; set; }
    public ActiveProductDto? Character { get; set; }
    public ActiveProductDto? Background { get; set; }
}
