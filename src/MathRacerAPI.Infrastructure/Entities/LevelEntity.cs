using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class LevelEntity
    {
        public int Id { get; set; }
        public int WorldId { get; set; }
        public int Number { get; set; }
        public int TermsCount { get; set; }
        public int VariablesCount { get; set; }
        public int ResultTypeId { get; set; }
        public ResultTypeEntity ResultType { get; set; } = null!;
        public WorldEntity World { get; set; } = null!;
    }
}
