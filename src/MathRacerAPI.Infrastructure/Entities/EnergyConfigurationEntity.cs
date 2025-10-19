using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class EnergyConfigurationEntity
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public int MaxAmount { get; set; }
    }
}
