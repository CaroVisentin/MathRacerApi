using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using Microsoft.Extensions.Logging;

namespace MathRacerAPI.Domain.UseCases
{
        public class ProcessWebhookUseCase
    {
            private readonly IPaymentService _paymentService;

            private readonly IPurchaseRepository _purchaseRepository;
            private readonly ICoinPackageRepository _coinPackageRepository;
            private readonly IPlayerRepository _playerRepository;

            private readonly ILogger<ProcessWebhookUseCase> _logger;

            public ProcessWebhookUseCase(
                IPaymentService paymentService,
                IPurchaseRepository purchaseRepository,
                ICoinPackageRepository coinPackageRepository,
                IPlayerRepository playerRepository,
                ILogger<ProcessWebhookUseCase> logger)
            {
                _paymentService = paymentService;
                _purchaseRepository = purchaseRepository;
                _coinPackageRepository = coinPackageRepository;
                _playerRepository = playerRepository;
                _logger = logger;
            }

        public async Task ExecuteAsync(JsonElement jsonPayload)
        {
        
            var paymentInfo = await _paymentService.ProcessWebhookNotificationAsync(jsonPayload);

            if (paymentInfo == null || paymentInfo.Status != "approved")
            {
                return;
            }

       
            if (string.IsNullOrEmpty(paymentInfo.ExternalReference))
            {
                _logger.LogWarning($"[WEBHOOK] Pago {paymentInfo.PaymentId} sin ExternalReference.");
                return;
            }

           
            if (await _purchaseRepository.ExistsByPaymentIdAsync(paymentInfo.PaymentId))
            {
                _logger.LogWarning($"[WEBHOOK] Pago {paymentInfo.PaymentId} ya procesado anteriormente (Check Rápido).");
                return;
            }

           
            var parts = paymentInfo.ExternalReference.Split('_');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int playerId) ||
                !int.TryParse(parts[1], out int packageId))
            {
                _logger.LogError($"[WEBHOOK] ExternalReference inválido: {paymentInfo.ExternalReference}");
                return;
            }

         
            try
            {
                var pack = await _coinPackageRepository.GetByIdAsync(packageId);
                if (pack == null)
                {
                    _logger.LogError($"[WEBHOOK] PackageId {packageId} no existe.");
                    return;
                }

                var purchase = new Purchase
                {
                    PlayerId = playerId,
                    CoinPackageId = packageId,
                    TotalAmount = pack.Price,
                    PaymentMethodId = 1, // 1 = MercadoPago
                    Date = DateTime.UtcNow,
                    PaymentId = paymentInfo.PaymentId
                };

                await _purchaseRepository.AddAsync(purchase);

                await _purchaseRepository.SaveChangesAsync();

                await _playerRepository.AddCoinsAsync(playerId, pack.CoinAmount);

                _logger.LogInformation($"[WEBHOOK] Éxito. Player {playerId} recibió {pack.CoinAmount} monedas. Pago: {paymentInfo.PaymentId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[WEBHOOK] Transacción ignorada para pago {paymentInfo.PaymentId} (Posible duplicado): {ex.Message}");
            }
        }
    }
}

    
