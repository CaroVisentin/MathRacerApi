using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases
{
    public class AcceptFriendRequestUseCase
    {
        private readonly IFriendshipRepository _repository;

        public AcceptFriendRequestUseCase(IFriendshipRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(int fromPlayerId, int toPlayerId)
        {
            var friendship = await _repository.GetFriendshipAsync(fromPlayerId, toPlayerId);
            if (friendship == null)
                throw new InvalidOperationException("Friend request does not exist.");

            var acceptedStatus = await _repository.GetRequestStatusByNameAsync("Aceptada");
            friendship.RequestStatus = acceptedStatus;
            friendship.Deleted = false;

            await _repository.UpdateFriendshipAsync(friendship);
        }
    }
}
