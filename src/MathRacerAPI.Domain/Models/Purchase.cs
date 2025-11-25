using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int CoinPackageId { get; set; }
        public double TotalAmount { get; set; }
        public DateTime Date { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentId { get; set; } 

    }
}
