using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;


namespace MathRacerAPI.Domain.Repositories
{
    public interface IFriendshipRepository
    {
        Task<Friendship?> GetFriendshipAsync(int playerId1, int playerId2);
        Task<IEnumerable<PlayerProfile>> GetFriendsAsync(int playerId);
        Task SendFriendRequestAsync(int fromPlayerId, int toPlayerId);
        Task AcceptFriendRequestAsync(int fromPlayerId, int toPlayerId);
        Task RejectFriendRequestAsync(int fromPlayerId, int toPlayerId);
    }

}
