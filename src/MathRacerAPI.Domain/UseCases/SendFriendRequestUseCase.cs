using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases
{
    public class SendFriendRequestUseCase
    {
        private readonly IFriendshipRepository _repository;

        public SendFriendRequestUseCase(IFriendshipRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(int fromPlayerId, int toPlayerId)
        {
            if (fromPlayerId == toPlayerId)
                throw new InvalidOperationException("You cannot send a friend request to yourself.");

            var existing = await _repository.GetFriendshipAsync(fromPlayerId, toPlayerId);

            var pendingStatus = await _repository.GetRequestStatusByNameAsync("Pendiente");
            var acceptedStatus = await _repository.GetRequestStatusByNameAsync("Aceptada");

            if (existing == null)
            {
                var friendship = new Friendship
                {
                    PlayerId1 = fromPlayerId,
                    PlayerId2 = toPlayerId,
                    RequestStatus = pendingStatus,
                };
                await _repository.AddFriendshipAsync(friendship);
                return;
            }

            if (existing.RequestStatus.Id == pendingStatus.Id && !existing.Deleted)
                throw new InvalidOperationException("There is already a pending friend request between these users.");

            if (existing.RequestStatus.Id == acceptedStatus.Id && !existing.Deleted)
                throw new InvalidOperationException("Users are already friends.");

            existing.RequestStatus = pendingStatus;
            existing.Deleted = false;

            await _repository.UpdateFriendshipAsync(existing);
        }
    }
}

