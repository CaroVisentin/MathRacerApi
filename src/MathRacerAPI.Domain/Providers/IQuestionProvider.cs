namespace MathRacerAPI.Domain.Providers;

/// <summary>
/// Proveedor de preguntas para el juego
/// </summary>
public interface IQuestionProvider
{

    /// <summary>
    /// Obtiene una lista de preguntas generadas dinámicamente basadas en los parámetros dados
    /// </summary>
    List<Models.Question> GetQuestions(Models.EquationParams p, int count);

    /// <summary>
    /// Genera una ecuación y sus opciones basadas en los parámetros dados
    /// </summary>
    Models.Question GenerateEquation(Models.EquationParams p);

}