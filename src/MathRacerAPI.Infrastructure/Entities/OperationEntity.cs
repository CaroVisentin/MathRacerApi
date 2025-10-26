using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class OperationEntity
    {
        public int Id { get; set; }
        public string Sign { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<WorldOperationEntity> WorldOperations { get; set; } = new List<WorldOperationEntity>();
    }
}
