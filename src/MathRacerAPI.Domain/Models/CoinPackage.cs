using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class CoinPackage
    {
        public int Id { get; set; }
        public int CoinAmount { get; set; }
        public double Price { get; set; }
        public string Description { get; set; } = "";
    }
}
