using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class CoinPackageEntity
    {
        public int Id { get; set; }
        public int CoinAmount { get; set; }
        public double Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public ICollection<PurchaseEntity> Purchases { get; set; } = new List<PurchaseEntity>();
    }
}
