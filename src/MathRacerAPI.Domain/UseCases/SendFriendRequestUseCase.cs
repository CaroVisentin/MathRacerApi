using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            await _repository.SendFriendRequestAsync(fromPlayerId, toPlayerId);
        }
    }
}
