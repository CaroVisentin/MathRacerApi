using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class PaymentMethodEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PaymentTypeId { get; set; }
        public int Installments { get; set; }
        public PaymentTypeEntity PaymentType { get; set; } = null!;
        public ICollection<PurchaseEntity> Purchases { get; set; } = new List<PurchaseEntity>();
    }

}
