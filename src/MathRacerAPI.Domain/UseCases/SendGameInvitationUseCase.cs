using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    /// <summary>
    /// Caso de uso para enviar una invitación de partida a un amigo
    /// </summary>
    public class SendGameInvitationUseCase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameInvitationRepository _invitationRepository;
        private readonly IGameRepository _gameRepository;
        private readonly ICreateCustomOnlineGameUseCase _createGameUseCase; 

        public SendGameInvitationUseCase(
            IPlayerRepository playerRepository,
            IGameInvitationRepository invitationRepository,
            IGameRepository gameRepository,
            ICreateCustomOnlineGameUseCase createGameUseCase) 
        {
            _playerRepository = playerRepository;
            _invitationRepository = invitationRepository;
            _gameRepository = gameRepository;
            _createGameUseCase = createGameUseCase;
        }

        public async Task<GameInvitation> ExecuteAsync(
            string inviterFirebaseUid,
            int invitedFriendId,
            string difficulty,
            string expectedResult)
        {
            // Obtener perfil del invitador
            var inviterProfile = await _playerRepository.GetByUidAsync(inviterFirebaseUid);
            if (inviterProfile == null)
                throw new NotFoundException("Perfil de invitador no encontrado");

            // Obtener perfil del amigo invitado
            var invitedProfile = await _playerRepository.GetByIdAsync(invitedFriendId);
            if (invitedProfile == null)
                throw new NotFoundException("Perfil de amigo no encontrado");

            // Crear partida SIN contraseña (partida por invitación controlada por GameInvitation)
            var gameName = $"{inviterProfile.Name} vs {invitedProfile.Name}";
            var game = await _createGameUseCase.ExecuteAsync(
                inviterFirebaseUid,
                gameName,
                isPrivate: false,
                password: null,
                difficulty,
                expectedResult
            );

            // Marcar la partida como originada por invitación y guardar
            game.IsFromInvitation = true;
            await _gameRepository.UpdateAsync(game);

            // Crear invitación
            var invitation = new GameInvitation
            {
                GameId = game.Id,
                InviterPlayerId = inviterProfile.Id,
                InviterPlayerName = inviterProfile.Name,
                InvitedPlayerId = invitedProfile.Id,
                InvitedPlayerName = invitedProfile.Name,
                GameName = gameName,
                Difficulty = difficulty,
                ExpectedResult = expectedResult
            };

            return await _invitationRepository.CreateAsync(invitation);
        }
    }
}