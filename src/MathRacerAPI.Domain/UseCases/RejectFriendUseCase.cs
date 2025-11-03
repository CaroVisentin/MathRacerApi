using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases
{
    public class RejectFriendRequestUseCase
    {
        private readonly IFriendshipRepository _repository;

        public RejectFriendRequestUseCase(IFriendshipRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(int fromPlayerId, int toPlayerId)
        {
            var friendship = await _repository.GetFriendshipAsync(fromPlayerId, toPlayerId);
            if (friendship == null)
                throw new InvalidOperationException("Friend request does not exist.");

            var rejectedStatus = await _repository.GetRequestStatusByNameAsync("Rechazada");
            friendship.RequestStatus = rejectedStatus;
            friendship.Deleted = false;

            await _repository.UpdateFriendshipAsync(friendship);
        }
    }
}
