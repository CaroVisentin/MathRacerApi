using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    /// <summary>
    /// Resultado de usar un wildcard
    /// </summary>
    public class WildcardUsageResult
    {
        public int WildcardId { get; set; }
        public int GameId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RemainingQuantity { get; set; }
        public List<int>? ModifiedOptions { get; set; }
        public int? NewQuestionIndex { get; set; }
        public bool DoubleProgressActive { get; set; }
        public SoloGame? Game { get; set; }
    }
}
