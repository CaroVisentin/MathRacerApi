using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    public class GoogleAuthUseCase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IPlayerRepository _playerRepository;
        public GoogleAuthUseCase(IFirebaseService firebaseService, IPlayerRepository playerRepository)
        {
            _firebaseService = firebaseService;
            _playerRepository = playerRepository;
        }

        public async Task<PlayerProfile?> ExecuteAsync(string idToken, string? username, string? email)
        {
            var uid = await _firebaseService.ValidateIdTokenAsync(idToken);
            if (uid == null) return null;
            var player = await _playerRepository.GetByUidAsync(uid);
            if (player == null && username != null && email != null)
            {
                // Crear jugador si no existe
                var newPlayer = new PlayerProfile
                {
                    Name = username,
                    Email = email,
                    Uid = uid,
                    LastLevelId = 1,
                    Points = 0,
                    Coins = 0
                };
                player = await _playerRepository.AddAsync(newPlayer);
            }
            return player;
        }
    }
}
