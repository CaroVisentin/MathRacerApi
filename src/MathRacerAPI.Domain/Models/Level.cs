using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class Level
    {
        public int Id { get; set; }
        public int WorldId { get; set; }
        public int Number { get; set; }
        public int TermsCount { get; set; }
        public int VariablesCount { get; set; }
        public string ResultType { get; set; } = null!;

    }
}
