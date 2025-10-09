namespace MathRacerAPI.Domain.Providers;

/// <summary>
/// Proveedor de preguntas para el juego
/// </summary>
public interface IQuestionProvider
{
    /// <summary>
    /// Obtiene una pregunta aleatoria
    /// </summary>
    /// <returns>Una pregunta con opciones múltiples</returns>
    Models.Question GetRandomQuestion();
    
    /// <summary>
    /// Obtiene todas las preguntas disponibles desde el archivo JSON
    /// </summary>
    /// <returns>Lista de preguntas JSON</returns>
    List<Models.JsonQuestion> GetQuestions();
}