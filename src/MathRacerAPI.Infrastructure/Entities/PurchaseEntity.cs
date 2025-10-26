using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class PurchaseEntity
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public DateTime Date { get; set; }
        public double TotalAmount { get; set; }
        public int PaymentMethodId { get; set; }
        public int CoinPackageId { get; set; }
        public PlayerEntity Player { get; set; } = null!;
        public PaymentMethodEntity PaymentMethod { get; set; } = null!;
        public CoinPackageEntity CoinPackage { get; set; } = null!;
    }

}
