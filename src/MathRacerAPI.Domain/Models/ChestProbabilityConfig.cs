using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Configuración de probabilidades para generar items de cofres
/// </summary>
public class ChestProbabilityConfig
{
    // Probabilidades de tipo de item (deben sumar 100)
    public double ProductProbability { get; set; } = 20.0;     
    public double CoinsProbability { get; set; } = 50.0;       
    public double WildcardProbability { get; set; } = 30.0;     

    // Rangos de monedas
    public int MinCoins { get; set; } = 100;
    public int MaxCoins { get; set; } = 1000;

    // Rangos de wildcards
    public int MinWildcards { get; set; } = 1;
    public int MaxWildcards { get; set; } = 3;

    // Compensación en monedas por producto duplicado según rareza
    public Dictionary<int, int> DuplicateCompensation { get; set; } = new()
    {
        { 1, 50 },  
        { 2, 150 },  
        { 3, 400 },
        { 4, 1000 }, 
        { 5, 5000 } 
    };
}
