using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para persistir una nueva compra confirmando los cambios en una única operación.
/// </summary>
public class PersistPurchaseUseCase
{
    private readonly IPurchaseRepository _purchaseRepository;

    public PersistPurchaseUseCase(IPurchaseRepository purchaseRepository)
    {
        _purchaseRepository = purchaseRepository ?? throw new ArgumentNullException(nameof(purchaseRepository));
    }

    /// <summary>
    /// Registra una nueva compra y confirma los cambios.
    /// </summary>
    /// <param name="purchase">Compra a persistir.</param>
    public async Task ExecuteAsync(Purchase purchase)
    {
        ArgumentNullException.ThrowIfNull(purchase);
        await _purchaseRepository.AddAsync(purchase);
        await _purchaseRepository.SaveChangesAsync();
    }
}
