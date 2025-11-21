using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    public class LoginPlayerUseCase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly Services.IFirebaseService _firebaseService;
        public LoginPlayerUseCase(IPlayerRepository playerRepository, Services.IFirebaseService firebaseService)
        {
            _playerRepository = playerRepository;
            _firebaseService = firebaseService;
        }

        public async Task<PlayerProfile?> ExecuteAsync(string email, string password, string? idToken = null)
        {
            if (string.IsNullOrEmpty(idToken)) return null;
            var validatedUid = await _firebaseService.ValidateIdTokenAsync(idToken);
            if (validatedUid == null) return null;

            var player = await _playerRepository.GetByEmailAsync(email);
            return player;
        }
    }
}
