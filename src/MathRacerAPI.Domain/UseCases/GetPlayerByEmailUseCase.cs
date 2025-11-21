using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases
{
   
    public class GetPlayerByEmailUseCase
    {
        private readonly IPlayerRepository _playerRepository;

        public GetPlayerByEmailUseCase(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        }

        public async Task<PlayerProfile?> ExecuteAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email no puede estar vacío.", nameof(email));

            return await _playerRepository.GetByEmailAsync(email);
        }
    }
}
