using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class PaymentInfo
    {
        public string? PaymentId { get; set; }
        public string? Status { get; set; } 
        public string? ExternalReference { get; set; }
    }
}
