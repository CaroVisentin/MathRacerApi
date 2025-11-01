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
            await _repository.RejectFriendRequestAsync(fromPlayerId, toPlayerId);
        }
    }
}
