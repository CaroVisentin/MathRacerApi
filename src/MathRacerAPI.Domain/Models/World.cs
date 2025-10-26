using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class World
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OptionsCount { get; set; }
        public int OptionRangeMin { get; set; }
        public int OptionRangeMax { get; set; }
        public int NumberRangeMin { get; set; }
        public int NumberRangeMax { get; set; }
        public int TimePerEquation { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public List<Level> Levels { get; set; } = new List<Level>();
        public List<string> Operations { get; set; } = new List<string>();
    }
}
