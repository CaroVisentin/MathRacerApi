using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    public class CreatePlayerUseCase
    {
        private readonly IPlayerRepository _playerRepository;

        public CreatePlayerUseCase(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<PlayerProfile> ExecuteAsync(string username, string email, string uid)
        {
            // Validaciones de entrada
            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("El nombre de usuario es requerido");

            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("El email es requerido");

            if (string.IsNullOrWhiteSpace(uid))
                throw new ValidationException("El UID es requerido");

            // Verificar si el email ya existe
            var existingPlayer = await _playerRepository.GetByEmailAsync(email);
            if (existingPlayer != null)
                throw new ValidationException($"El email '{email}' ya está registrado");

            var playerProfile = new PlayerProfile
            {
                Name = username,
                Email = email,
                Uid = uid,
            };

            // Si hay error de DB, la excepción se propaga automáticamente
            var createdPlayer = await _playerRepository.AddAsync(playerProfile);

            return createdPlayer;
        }
    }
}
