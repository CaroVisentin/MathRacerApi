using System.Collections.Generic;

namespace MathRacerAPI.Domain.Models
{
    /// <summary>
    /// Modelo de dominio que representa todos los mundos y el progreso del jugador
    /// </summary>
    public class PlayerWorlds
    {
        /// <summary>
        /// Lista de todos los mundos del juego
        /// </summary>
        public List<World> Worlds { get; set; } = new();

        /// <summary>
        /// ID del último mundo disponible para el jugador
        /// </summary>
        public int LastAvailableWorldId { get; set; }
    }
}
