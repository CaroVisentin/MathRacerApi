using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class GarageItemsResponse
    {
        public List<GarageItem> Items { get; set; } = new List<GarageItem>();
        public GarageItem? ActiveItem { get; set; }
        public string ItemType { get; set; } = string.Empty;
    }
}