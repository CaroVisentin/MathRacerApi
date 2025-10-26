using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class WorldEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OptionsCount { get; set; }
        public int OptionRangeMin { get; set; }
        public int OptionRangeMax { get; set; }
        public int NumberRangeMin { get; set; }
        public int NumberRangeMax { get; set; }
        public int TimePerEquation { get; set; }
        public int DifficultyId { get; set; }
        public DifficultyEntity Difficulty { get; set; } = null!;
        public ICollection<LevelEntity> Levels { get; set; } = new List<LevelEntity>();
        public ICollection<WorldOperationEntity> WorldOperations { get; set; } = new List<WorldOperationEntity>();

    }
}
