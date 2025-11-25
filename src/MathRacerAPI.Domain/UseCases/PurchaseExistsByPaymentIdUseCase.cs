using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para verificar si un pago ya fue procesado previamente.
/// </summary>
public class PurchaseExistsByPaymentIdUseCase
{
    private readonly IPurchaseRepository _purchaseRepository;

    public PurchaseExistsByPaymentIdUseCase(IPurchaseRepository purchaseRepository)
    {
        _purchaseRepository = purchaseRepository ?? throw new ArgumentNullException(nameof(purchaseRepository));
    }

    /// <summary>
    /// Determina si existe una compra asociada al identificador de pago de Mercado Pago.
    /// </summary>
    /// <param name="paymentId">Identificador del pago provisto por Mercado Pago.</param>
    /// <returns><c>true</c> si la compra ya fue registrada; caso contrario <c>false</c>.</returns>
    public Task<bool> ExecuteAsync(string paymentId)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            throw new ArgumentException("PaymentId requerido", nameof(paymentId));
        }

        return _purchaseRepository.ExistsByPaymentIdAsync(paymentId);
    }
}
