using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class PlayerWildcardEntity
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int WildcardId { get; set; }
        public int Quantity { get; set; }
        public PlayerEntity Player { get; set; } = null!;
        public WildcardEntity Wildcard { get; set; } = null!;
    }

}
