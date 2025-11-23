using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories
{
    public class GameInvitationRepository : IGameInvitationRepository
    {
        private readonly MathiRacerDbContext _context;
        private readonly IGameRepository _gameRepository;

        public GameInvitationRepository(MathiRacerDbContext context, IGameRepository gameRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        }

        public async Task<GameInvitation> CreateAsync(GameInvitation invitation)
        {
            var entity = new GameInvitationEntity
            {
                GameId = invitation.GameId,
                InviterPlayerId = invitation.InviterPlayerId,
                InvitedPlayerId = invitation.InvitedPlayerId,
                InvitationStatusId = (int)InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameInvitations.Add(entity);
            await _context.SaveChangesAsync();

            invitation.Id = entity.Id;
            invitation.CreatedAt = entity.CreatedAt;
            invitation.Status = InvitationStatus.Pending;

            return invitation;
        }

        public async Task<GameInvitation?> GetByIdAsync(int id)
        {
            var entity = await _context.GameInvitations
                .Include(gi => gi.InviterPlayer)
                .Include(gi => gi.InvitedPlayer)
                .Include(gi => gi.InvitationStatus)
                .FirstOrDefaultAsync(gi => gi.Id == id);

            if (entity == null) return null;

            return await MapToDomainWithGameInfo(entity);
        }

        public async Task<List<GameInvitation>> GetPendingInvitationsForPlayerAsync(int playerId)
        {
            var entities = await _context.GameInvitations
                .Include(gi => gi.InviterPlayer)
                .Include(gi => gi.InvitedPlayer)
                .Include(gi => gi.InvitationStatus)
                .Where(gi => gi.InvitedPlayerId == playerId && gi.InvitationStatusId == (int)InvitationStatus.Pending)
                .OrderByDescending(gi => gi.CreatedAt)
                .ToListAsync();

            var invitations = new List<GameInvitation>();
            foreach (var entity in entities)
            {
                var invitation = await MapToDomainWithGameInfo(entity);
                if (invitation != null)
                {
                    invitations.Add(invitation);
                }
            }

            return invitations;
        }

        public async Task UpdateStatusAsync(int invitationId, InvitationStatus status)
        {
            await _context.GameInvitations
                .Where(gi => gi.Id == invitationId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(gi => gi.InvitationStatusId, (int)status)
                    .SetProperty(gi => gi.RespondedAt, DateTime.UtcNow));
        }

        public async Task DeleteAsync(int invitationId)
        {
            await _context.GameInvitations
                .Where(gi => gi.Id == invitationId)
                .ExecuteDeleteAsync();
        }

        public async Task<GameInvitation?> GetByGameIdAsync(int gameId)
        {
            var entity = await _context.GameInvitations
                .Include(gi => gi.InviterPlayer)
                .Include(gi => gi.InvitedPlayer)
                .Include(gi => gi.InvitationStatus)
                .FirstOrDefaultAsync(gi => gi.GameId == gameId);

            if (entity == null) return null;

            return await MapToDomainWithGameInfo(entity);
        }

        private async Task<GameInvitation?> MapToDomainWithGameInfo(GameInvitationEntity entity)
        {
            // Obtener información del juego
            var game = await _gameRepository.GetByIdAsync(entity.GameId);

            return new GameInvitation
            {
                Id = entity.Id,
                GameId = entity.GameId,
                InviterPlayerId = entity.InviterPlayerId,
                InviterPlayerName = entity.InviterPlayer.Name,
                InvitedPlayerId = entity.InvitedPlayerId,
                InvitedPlayerName = entity.InvitedPlayer.Name,
                Status = (InvitationStatus)entity.InvitationStatusId,
                CreatedAt = entity.CreatedAt,
                RespondedAt = entity.RespondedAt,
                // Información del juego
                GameName = game?.Name ?? $"{entity.InviterPlayer.Name} vs {entity.InvitedPlayer.Name}",
                Difficulty = DetermineDifficulty(game),
                ExpectedResult = game?.ExpectedResult ?? "MAYOR"
            };
        }

        private string DetermineDifficulty(Game? game)
        {
            if (game == null) return "facil";

            // Determinar dificultad basándose en los parámetros del juego
            var questionCount = game.Questions.Count;
            var conditionToWin = game.ConditionToWin;

            if (conditionToWin >= 8 || questionCount >= 15)
                return "dificil";
            else if (conditionToWin >= 6 || questionCount >= 12)
                return "medio";
            else
                return "facil";
        }
    }
}