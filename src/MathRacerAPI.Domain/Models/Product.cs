using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int ProductType { get; set; }
        public int RarityId { get; set; }
        public string RarityName { get; set; } = string.Empty;
        public string RarityColor { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new List<Player>();

    }
}
