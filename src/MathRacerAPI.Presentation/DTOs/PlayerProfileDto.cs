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

        public ProductDto? Car { get; set; }
        public ProductDto? Background { get; set; }
        public ProductDto? Character { get; set; }

    }

}
