using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class FriendshipEntity
    {
        public int Id { get; set; }
        public int PlayerId1 { get; set; }
        public int PlayerId2 { get; set; }
        public int RequestStatusId { get; set; }
        public PlayerEntity Player1 { get; set; } = null!;
        public PlayerEntity Player2 { get; set; } = null!;
        public RequestStatusEntity RequestStatus { get; set; } = null!;
    }

}
