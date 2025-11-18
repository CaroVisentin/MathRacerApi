using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Infrastructure.Entities;
using MercadoPago.Client.MerchantOrder;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace MathRacerAPI.Presentation.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly PurchaseExistsByPaymentIdUseCase _purchaseExistsUseCase;
        private readonly PersistPurchaseUseCase _persistPurchaseUseCase;
        private readonly GetCoinPackageUseCase _getCoinPackageUseCase;
        private readonly IPlayerRepository _playerRepo;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            PurchaseExistsByPaymentIdUseCase purchaseExistsUseCase,
            PersistPurchaseUseCase persistPurchaseUseCase,
            GetCoinPackageUseCase getCoinPackageUseCase,
            IPlayerRepository playerRepo,
            ILogger<WebhookController> logger)
        {
            _purchaseExistsUseCase = purchaseExistsUseCase;
            _persistPurchaseUseCase = persistPurchaseUseCase;
            _getCoinPackageUseCase = getCoinPackageUseCase;
            _playerRepo = playerRepo;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] object body)
        {
            try
            {
                if (body == null)
                {
                    _logger.LogWarning("[WEBHOOK] Body vacío");
                    return Ok();
                }

                var json = JObject.Parse(body.ToString());
                _logger.LogInformation("[WEBHOOK] JSON: " + json.ToString());

                string type = json["type"]?.ToString();
                if (string.IsNullOrEmpty(type))
                {
                    _logger.LogWarning("[WEBHOOK] Webhook sin 'type'");
                    return Ok();
                }

                string paymentIdStr = json["data"]?["id"]?.ToString() ?? json["id"]?.ToString();
                if (!long.TryParse(paymentIdStr, out long paymentId))
                {
                    _logger.LogWarning("[WEBHOOK] No se pudo extraer paymentId");
                    return Ok();
                }

                _logger.LogInformation($"[WEBHOOK] Pago ID={paymentId}");

                var paymentClient = new PaymentClient();
                var payment = await paymentClient.GetAsync(paymentId);

                if (payment.Status != "approved")
                {
                    _logger.LogInformation($"Pago no aprobado: {payment.Status}");
                    return Ok();
                }

                bool exists = await _purchaseExistsUseCase.ExecuteAsync(paymentId.ToString());
                if (exists)
                {
                    _logger.LogWarning($"Pago ya procesado");
                    return Ok();
                }

                if (string.IsNullOrEmpty(payment.ExternalReference))
                {
                    _logger.LogError("[WEBHOOK] ExternalReference vacío: no se pudo obtener playerId o coinPackageId");
                    return Ok();
                }

                var parts = payment.ExternalReference.Split('_');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int playerId) || !int.TryParse(parts[1], out int coinPackageId))
                {
                    _logger.LogError("[WEBHOOK] ExternalReference inválido: " + payment.ExternalReference);
                    return Ok();
                }

                var pack = await _getCoinPackageUseCase.ExecuteAsync(coinPackageId);

                var purchase = new Purchase
                {
                    PlayerId = playerId,
                    CoinPackageId = pack.Id,
                    TotalAmount = pack.Price,
                    PaymentMethodId = 1,
                    Date = DateTime.UtcNow,
                    PaymentId = paymentId.ToString()
                };

                await _persistPurchaseUseCase.ExecuteAsync(purchase);
                await _playerRepo.AddCoinsAsync(playerId, pack.CoinAmount);

                _logger.LogInformation("[WEBHOOK] Compra procesada correctamente");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WEBHOOK ERROR]");
                return Ok();
            }
 
        }
    }
}

