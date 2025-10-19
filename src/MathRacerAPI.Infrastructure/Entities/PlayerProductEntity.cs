using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class PlayerProductEntity
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int ProductId { get; set; }
        public bool IsActive { get; set; }
        public PlayerEntity Player { get; set; } = null!;
        public ProductEntity Product { get; set; } = null!;
    }

}
