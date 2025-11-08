using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class RarityEntity
    {
        public int Id { get; set; }
        public string Rarity { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public double Probability { get; set; }
        public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
    }
}
