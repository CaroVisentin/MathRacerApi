using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs
{
    /// <summary>
    /// DTO para la respuesta completa de mundos del jugador
    /// </summary>
    public class PlayerWorldsResponseDto
    {
        /// <summary>
        /// Lista de todos los mundos disponibles en el juego
        /// </summary>
        public List<WorldDto> Worlds { get; set; } = new();

        /// <summary>
        /// ID del último mundo disponible/completado por el jugador
        /// </summary>
        public int LastAvailableWorldId { get; set; }
    }

    /// <summary>
    /// DTO para un mundo individual
    /// </summary>
    public class WorldDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int TimePerEquation { get; set; }
        public List<string> Operations { get; set; } = new();
        public int OptionsCount { get; set; }
    }
}
