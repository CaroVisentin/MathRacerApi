using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models;

/// <summary>
/// Resultado del procesamiento de una respuesta en modo individual
/// </summary>
public class SoloAnswerResult
{
    public SoloGame Game { get; set; } = new();
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    public int PlayerAnswer { get; set; }  
    public Question? NextQuestion { get; set; }
}
