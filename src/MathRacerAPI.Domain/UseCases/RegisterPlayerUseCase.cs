using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    public class RegisterPlayerUseCase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly Services.IFirebaseService _firebaseService;
        public RegisterPlayerUseCase(IPlayerRepository playerRepository, Services.IFirebaseService firebaseService)
        {
            _playerRepository = playerRepository;
            _firebaseService = firebaseService;
        }

        public async Task<PlayerProfile?> ExecuteAsync(string username, string email, string? uid = null, string? idToken = null)
        {
            if (string.IsNullOrEmpty(idToken)) return null;
            var validatedUid = await _firebaseService.ValidateIdTokenAsync(idToken);
            if (validatedUid == null) return null;

            var existing = await _playerRepository.GetByEmailAsync(email);
            if (existing != null) return null;
            var playerProfile = new PlayerProfile
            {
                Name = username,
                Email = email,
                Uid = uid ?? validatedUid,
            };
            var created = await _playerRepository.AddAsync(playerProfile);
            return created;
        }
    }
}
