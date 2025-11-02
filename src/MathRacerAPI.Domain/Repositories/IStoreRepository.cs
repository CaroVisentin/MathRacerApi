using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Repositories;

public interface IStoreRepository
{
    Task<List<StoreItem>> GetProductsByTypeAsync(int productTypeId, int playerId);
    Task<StoreItem?> GetProductByIdAsync(int productId, int playerId);
    Task<bool> PlayerOwnsProductAsync(int playerId, int productId);
    Task<bool> PurchaseProductAsync(int playerId, int productId, decimal price);
}
