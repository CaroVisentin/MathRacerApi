﻿using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    /// <summary>
    /// Caso de uso para obtener un jugador por su ID
    /// </summary>
    public class GetPlayerByIdUseCase
    {
        private readonly IPlayerRepository _playerRepository;

        public GetPlayerByIdUseCase(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        /// <summary>
        /// Ejecuta la lógica de obtención de un jugador por su UID
        /// </summary>
        /// <param name="uid">UID del jugador</param>
        /// <returns>Jugador encontrado</returns>
        /// <exception cref="ValidationException">Cuando el UID es inválido</exception>
        /// <exception cref="NotFoundException">Cuando el jugador no existe</exception>
        public async Task<PlayerProfile> ExecuteByUidAsync(string uid)
        {
            // Validación del UID
            if (string.IsNullOrWhiteSpace(uid))
                throw new ValidationException("El UID es requerido");

            // Obtener jugador del repositorio
            var playerProfile = await _playerRepository.GetByUidAsync(uid);

            // Lanzar excepción si no existe
            if (playerProfile == null)
                throw new NotFoundException("No se encontró un jugador con el UID proporcionado.");

            return playerProfile;
        }
    }
}
