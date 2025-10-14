using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class ProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int SubproductId { get; set; }
        public SubproductEntity Subproduct { get; set; } = null!;
        public ICollection<PlayerProductEntity> PlayerProducts { get; set; } = new List<PlayerProductEntity>();
    }

}
