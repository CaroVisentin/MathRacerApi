using MathRacerAPI.Domain.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MathRacerAPI.Presentation.DTOs
{
    public class PlayerProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int LastLevelId { get; set; }
        public int Points { get; set; }
        public int Coins { get; set; }

        // En el perfil del jugador sólo exponemos el Id del producto
        public ActiveProductDto? Car { get; set; }
        public ActiveProductDto? Background { get; set; }
        public ActiveProductDto? Character { get; set; }

    }

}
