using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class InventoryEntity
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int Wildcard1Count { get; set; }
        public int Wildcard2Count { get; set; }
        public int Wildcard3Count { get; set; }
        public PlayerEntity Player { get; set; } = null!;
    }

}
