using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Entities
{
    public class PlayerEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public double Coins { get; set; }
        public int LastLevelId { get; set; }
        public int InventoryId { get; set; }
        public int EnergyId { get; set; }
        public int Points { get; set; }
        public bool Deleted { get; set; }
        public LevelEntity LastLevel { get; set; } = null!;
        public InventoryEntity Inventory { get; set; } = null!;
        public EnergyEntity Energy { get; set; } = null!;
        public ICollection<FriendshipEntity> Friendships1 { get; set; } = new List<FriendshipEntity>();
        public ICollection<FriendshipEntity> Friendships2 { get; set; } = new List<FriendshipEntity>();
        public ICollection<PlayerProductEntity> PlayerProducts { get; set; } = new List<PlayerProductEntity>();
    }

}
