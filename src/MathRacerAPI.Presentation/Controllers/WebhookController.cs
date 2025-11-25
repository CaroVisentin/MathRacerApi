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

                var json = JObject.Parse(body.ToString());
                _logger.LogInformation("[WEBHOOK] JSON: " + json.ToString());

                string topic = json["topic"]?.ToString() ?? json["type"]?.ToString();

                if (string.IsNullOrEmpty(topic))
                {
                    _logger.LogWarning("[WEBHOOK] No topic/type found");
                    return Ok();
                }

                switch (topic)
                {
                    case "payment":
                        return await HandlePaymentWebhook(json);

                    case "merchant_order":
                        return await HandleMerchantOrderWebhook(json);

                    default:
                        _logger.LogWarning($"[WEBHOOK] Unhandled topic: {topic}");
                        return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WEBHOOK ERROR]");
                return Ok();
            }
        }

        private async Task<IActionResult> HandlePaymentWebhook(JObject json)
        {
            // MP puede mandar el id en distintos campos
            string paymentIdStr =
                json["data"]?["id"]?.ToString()
                ?? json["id"]?.ToString()
                ?? json["resource"]?.ToString(); // <--- acá agarramos lo que te está llegando

            if (string.IsNullOrWhiteSpace(paymentIdStr))
            {
                _logger.LogWarning("[WEBHOOK-PAYMENT] paymentIdStr is null or empty");
                return Ok();
            }

            long paymentId;

            // Si viene directo como número (como en tu log: "134589415559")
            if (!long.TryParse(paymentIdStr, out paymentId))
            {
                // Si alguna vez viniera como URL, ejemplo:
                // "https://api.mercadopago.com/v1/payments/134589415559"
                var lastSegment = paymentIdStr
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .LastOrDefault();

                if (lastSegment == null || !long.TryParse(lastSegment, out paymentId))
                {
                    _logger.LogWarning($"[WEBHOOK-PAYMENT] Could not parse paymentId from: {paymentIdStr}");
                    return Ok();
                }
            }

            _logger.LogInformation($"[WEBHOOK-PAYMENT] Using paymentId: {paymentId}");

            var client = new PaymentClient();
            var payment = await client.GetAsync(paymentId);

            if (payment.Status != "approved")
            {
                _logger.LogInformation($"[PAYMENT] Not approved: {payment.Status}");
                return Ok();
            }

            return await ProcessPurchase(paymentId, payment.ExternalReference);
        }


        private async Task<IActionResult> HandleMerchantOrderWebhook(JObject json)
        {
            string orderIdStr = json["data"]?["id"]?.ToString() ?? json["id"]?.ToString();

            if (!long.TryParse(orderIdStr, out long orderId))
            {
                _logger.LogWarning("[WEBHOOK-ORDER] Could not extract orderId");
                return Ok();
            }

            var orderClient = new MerchantOrderClient();
            var order = await orderClient.GetAsync(orderId);

            // Buscar si tiene algún pago aprobado
            var approvedPayment = order.Payments?
                .FirstOrDefault(p => p.Status == "approved");

            if (approvedPayment == null)
            {
                _logger.LogInformation("[ORDER] No approved payments yet");
                return Ok();
            }

            long paymentId = approvedPayment.Id ?? 0;

            // Obtener el payment completo
            var paymentClient = new PaymentClient();
            var payment = await paymentClient.GetAsync(paymentId);

            return await ProcessPurchase(paymentId, payment.ExternalReference);
        }


        private async Task<IActionResult> ProcessPurchase(long paymentId, string externalRef)
        {
            if (string.IsNullOrEmpty(externalRef))
            {
                _logger.LogError("[WEBHOOK] ExternalReference empty");
                return Ok();
            }

            // evitar doble procesamiento
            bool exists = await _purchaseExistsUseCase.ExecuteAsync(paymentId.ToString());
            if (exists)
            {
                _logger.LogWarning("[WEBHOOK] Payment already processed");
                return Ok();
            }

            var parts = externalRef.Split('_');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int playerId) ||
                !int.TryParse(parts[1], out int packageId))
            {
                _logger.LogError("[WEBHOOK] Invalid ExternalReference: " + externalRef);
                return Ok();
            }

            var pack = await _getCoinPackageUseCase.ExecuteAsync(packageId);

            var purchase = new Purchase
            {
                PlayerId = playerId,
                CoinPackageId = packageId,
                TotalAmount = pack.Price,
                PaymentMethodId = 1,
                Date = DateTime.UtcNow,
                PaymentId = paymentId.ToString()
            };

            await _persistPurchaseUseCase.ExecuteAsync(purchase);
            await _addCoinsToPlayerUseCase.ExecuteAsync(playerId, pack.CoinAmount);

            _logger.LogInformation("[WEBHOOK] Purchase processed OK");

            return Ok();
        }


    }
}

