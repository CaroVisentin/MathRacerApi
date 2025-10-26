using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class WorldOperationEntity
    {
        public int Id { get; set; }
        public int WorldId { get; set; }
        public WorldEntity World { get; set; } = null!;
        public int OperationId { get; set; }
        public OperationEntity Operation { get; set; } = null!;
    }
}
