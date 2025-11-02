using MathRacerAPI.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories
{
    public interface IGarageRepository
    {
        Task<GarageItemsResponse> GetPlayerItemsByTypeAsync(int playerId, string productType);
        Task<bool> ActivatePlayerItemAsync(int playerId, int productId, string productType);
        Task<GarageItem?> GetActiveItemByTypeAsync(int playerId, string productType);
    }
}