using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases
{
    public class GetFriendsUseCase
    {
        private readonly IFriendshipRepository _repository;

        public GetFriendsUseCase(IFriendshipRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PlayerProfile>> ExecuteAsync(int playerId)
        {
            return await _repository.GetFriendsAsync(playerId);
        }
    }
}
