using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Interface para obtener autos de la tienda
/// </summary>
public interface IGetStoreCarsUseCase
{
    Task<List<StoreItem>> ExecuteAsync(int playerId);
}

/// <summary>
/// Interface para obtener personajes de la tienda
/// </summary>
public interface IGetStoreCharactersUseCase
{
    Task<List<StoreItem>> ExecuteAsync(int playerId);
}

/// <summary>
/// Interface para obtener fondos de la tienda
/// </summary>
public interface IGetStoreBackgroundsUseCase
{
    Task<List<StoreItem>> ExecuteAsync(int playerId);
}

/// <summary>
/// Interface para comprar productos de la tienda
/// </summary>
public interface IPurchaseStoreItemUseCase
{
    Task<PurchaseResult> ExecuteAsync(int playerId, int productId);
}