using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        public int PlayerId1 { get; set; }

        public int PlayerId2 { get; set; }

        public RequestStatus RequestStatus { get; set; } = null!;

        public PlayerProfile? Player1 { get; set; }
        public PlayerProfile? Player2 { get; set; }

    }
}
