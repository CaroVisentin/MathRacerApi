using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Modelo de dominio para el perfil del jugador (autenticación y datos persistentes)
/// </summary>
public class PlayerProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public int? LastLevelId { get; set; }
    public int Points { get; set; }
    public int Coins { get; set; }

}
