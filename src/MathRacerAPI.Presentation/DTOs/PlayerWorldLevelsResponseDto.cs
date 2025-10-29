using System.Collections.Generic;

namespace MathRacerAPI.Presentation.DTOs
{
    /// <summary>
    /// DTO de respuesta para niveles de un mundo con progreso del jugador
    /// </summary>
    public class PlayerWorldLevelsResponseDto
    {
        public string WorldName { get; set; } = string.Empty;
        public List<LevelDto> Levels { get; set; } = new();
        public int LastCompletedLevelId { get; set; }
    }
}
