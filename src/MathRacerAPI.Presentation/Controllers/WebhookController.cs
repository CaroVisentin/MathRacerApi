using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Infrastructure.Entities;
using MercadoPago.Client.MerchantOrder;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly PurchaseExistsByPaymentIdUseCase _purchaseExistsUseCase;
        private readonly PersistPurchaseUseCase _persistPurchaseUseCase;
        private readonly GetCoinPackageUseCase _getCoinPackageUseCase;
        private readonly AddCoinsToPlayerUseCase _addCoinsToPlayerUseCase;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            PurchaseExistsByPaymentIdUseCase purchaseExistsUseCase,
            PersistPurchaseUseCase persistPurchaseUseCase,
            GetCoinPackageUseCase getCoinPackageUseCase,
            AddCoinsToPlayerUseCase addCoinsToPlayerUseCase,
            ILogger<WebhookController> logger)
        {
            _purchaseExistsUseCase = purchaseExistsUseCase;
            _persistPurchaseUseCase = persistPurchaseUseCase;
            _getCoinPackageUseCase = getCoinPackageUseCase;
            _addCoinsToPlayerUseCase = addCoinsToPlayerUseCase;
            _logger = logger;
        }

       [SwaggerOperation(
            Summary = "Recibe webhook de pago",
            Description = "Recibe notificaciones de pago de MercadoPago y procesa las compras correspondientes."
        )]
        [SwaggerResponse(200, "Webhook recibido y procesado exitosamente")]
        [SwaggerResponse(400, "Payload de webhook inválido")]

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] object body)
        {
            try
            {
                if (body == null)
                {
                    _logger.LogWarning("[WEBHOOK] Empty body");
                    return Ok();
                }

                var bodyString = body?.ToString();
                if (string.IsNullOrWhiteSpace(bodyString))
                {
                    _logger.LogWarning("[WEBHOOK] Invalid body");
                    return Ok();
                }

                var json = JObject.Parse(bodyString);
                _logger.LogInformation("[WEBHOOK] JSON: " + json.ToString());

                string? type = json["type"]?.ToString();
                if (string.IsNullOrEmpty(type))
                {
                    _logger.LogWarning("[WEBHOOK] Webhook without 'type'");
                    return Ok();
                }

                string? paymentIdStr = json["data"]?["id"]?.ToString() ?? json["id"]?.ToString();
                if (!long.TryParse(paymentIdStr, out long paymentId))
                {
                    _logger.LogWarning("[WEBHOOK] Could not extract paymentId");
                    return Ok();
                }

                _logger.LogInformation($"[WEBHOOK] Payment ID={paymentId}");
              
                var paymentClient = new PaymentClient();
                var payment = await paymentClient.GetAsync(paymentId);

                if (payment.Status != "approved")
                {
                    _logger.LogInformation($"Payment not approved: {payment.Status}");
                    return Ok();
                }

                bool exists = await _purchaseExistsUseCase.ExecuteAsync(paymentId.ToString());
                if (exists)
                {
                    _logger.LogWarning("Payment already processed");
                    return Ok();
                }

                if (string.IsNullOrEmpty(payment.ExternalReference))
                {
                    _logger.LogError("[WEBHOOK] ExternalReference empty: could not obtain playerId or coinPackageId");
                    return Ok();
                }

                var parts = payment.ExternalReference.Split('_');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int playerId) || !int.TryParse(parts[1], out int coinPackageId))
                {
                    _logger.LogError("[WEBHOOK] ExternalReference invalid: " + payment.ExternalReference);
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
                await _addCoinsToPlayerUseCase.ExecuteAsync(playerId, pack.CoinAmount);

                _logger.LogInformation("[WEBHOOK] Purchase processed successfully");

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

