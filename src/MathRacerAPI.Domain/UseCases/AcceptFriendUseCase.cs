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
            await _repository.AcceptFriendRequestAsync(fromPlayerId, toPlayerId);
        }
    }
}
