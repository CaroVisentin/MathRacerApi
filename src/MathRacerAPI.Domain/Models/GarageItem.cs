using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class GarageItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public bool IsOwned { get; set; }
        public bool IsActive { get; set; }
    }
}