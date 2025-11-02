using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    public class GetPlayerGarageItemsUseCase
    {
        private readonly IGarageRepository _garageRepository;

        public GetPlayerGarageItemsUseCase(IGarageRepository garageRepository)
        {
            _garageRepository = garageRepository;
        }

        public async Task<GarageItemsResponse> ExecuteAsync(int playerId, string itemType)
        {
            if (playerId <= 0)
                throw new ArgumentException("Player ID must be greater than 0", nameof(playerId));

            if (string.IsNullOrWhiteSpace(itemType))
                throw new ArgumentException("Item type cannot be null or empty", nameof(itemType));

            // Normalize and validate item type (case-insensitive)
            var normalizedItemType = NormalizeProductType(itemType);
            if (normalizedItemType == null)
                throw new ArgumentException($"Invalid item type. Valid types are: Auto, Personaje, Fondo (case-insensitive)", nameof(itemType));

            return await _garageRepository.GetPlayerItemsByTypeAsync(playerId, normalizedItemType);
        }

        private string? NormalizeProductType(string input)
        {
            return input?.ToLower() switch
            {
                "auto" => "Auto",
                "personaje" => "Personaje", 
                "fondo" => "Fondo",
                _ => null
            };
        }
    }
}