using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases
{
    public class DeleteFriendUseCase
    {
        private readonly IFriendshipRepository _repository;

        public DeleteFriendUseCase(IFriendshipRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(int playerId1, int playerId2)
        {
            var friendship = await _repository.GetFriendshipAsync(playerId1, playerId2);
            if (friendship == null)
                throw new InvalidOperationException("Friendship does not exist.");

            var acceptedStatus = await _repository.GetRequestStatusByNameAsync("Aceptada");

            if (friendship.RequestStatus.Id != acceptedStatus.Id)
                throw new InvalidOperationException("Only accepted friends can be deleted.");

            friendship.Deleted = true;
            await _repository.UpdateFriendshipAsync(friendship);
        }
    }

}
