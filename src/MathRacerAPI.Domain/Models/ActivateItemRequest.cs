using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class ActivateItemRequest
    {
        public int PlayerId { get; set; }
        public int ProductId { get; set; }
        public string ProductType { get; set; } = string.Empty;
    }
}