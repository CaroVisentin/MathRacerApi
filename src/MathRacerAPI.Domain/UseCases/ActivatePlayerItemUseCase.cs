using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    public class ActivatePlayerItemUseCase
    {
        private readonly IGarageRepository _garageRepository;

        public ActivatePlayerItemUseCase(IGarageRepository garageRepository)
        {
            _garageRepository = garageRepository;
        }

        public async Task<bool> ExecuteAsync(ActivateItemRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.PlayerId <= 0)
                throw new ArgumentException("Player ID must be greater than 0", nameof(request.PlayerId));

            if (request.ProductId <= 0)
                throw new ArgumentException("Product ID must be greater than 0", nameof(request.ProductId));

            if (string.IsNullOrWhiteSpace(request.ProductType))
                throw new ArgumentException("Product type cannot be null or empty", nameof(request.ProductType));

            // Normalize and validate product type (case-insensitive)
            var normalizedProductType = NormalizeProductType(request.ProductType);
            if (normalizedProductType == null)
                throw new ArgumentException($"Invalid product type. Valid types are: Auto, Personaje, Fondo (case-insensitive)", nameof(request.ProductType));

            return await _garageRepository.ActivatePlayerItemAsync(request.PlayerId, request.ProductId, normalizedProductType);
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