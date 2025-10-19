using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class EnergyEntity
    {
        public int PlayerId { get; set; }
        public int Amount { get; set; }
        public DateTime LastConsumptionDate { get; set; }
        public PlayerEntity Player { get; set; } = null!;
    }

}
