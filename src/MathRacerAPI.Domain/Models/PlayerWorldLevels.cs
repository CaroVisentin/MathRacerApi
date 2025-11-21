using System.Collections.Generic;

namespace MathRacerAPI.Domain.Models
{
    /// <summary>
    /// Modelo de dominio que representa todos los niveles de un mundo y el progreso del jugador
    /// </summary>
    public class PlayerWorldLevels
    {
        /// <summary>
        /// Nombre del mundo
        /// </summary>
        public string WorldName { get; set; } = string.Empty;

        /// <summary>
        /// Lista de todos los niveles del mundo
        /// </summary>
        public List<Level> Levels { get; set; } = new();

        /// <summary>
        /// ID del último nivel completado por el jugador (0 si no ha completado ninguno)
        /// </summary>
        public int LastCompletedLevelId { get; set; }

    }
}
