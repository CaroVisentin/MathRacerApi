using System.Text.Json;
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
        private readonly ProcessWebhookUseCase _processWebhookUseCase; 
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            PurchaseExistsByPaymentIdUseCase purchaseExistsUseCase,
            PersistPurchaseUseCase persistPurchaseUseCase,
            GetCoinPackageUseCase getCoinPackageUseCase,
            AddCoinsToPlayerUseCase addCoinsToPlayerUseCase,
            ProcessWebhookUseCase processWebhookUseCase,
            ILogger<WebhookController> logger)
        {
            _purchaseExistsUseCase = purchaseExistsUseCase;
            _persistPurchaseUseCase = persistPurchaseUseCase;
            _getCoinPackageUseCase = getCoinPackageUseCase;
            _addCoinsToPlayerUseCase = addCoinsToPlayerUseCase;
            _processWebhookUseCase = processWebhookUseCase;
            _logger = logger;
        }

       [SwaggerOperation(
            Summary = "Recibe webhook de pago",
            Description = "Recibe notificaciones de pago de MercadoPago y procesa las compras correspondientes."
        )]
        [SwaggerResponse(200, "Webhook recibido y procesado exitosamente")]
        [SwaggerResponse(400, "Payload de webhook inválido")]

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] JsonElement body)
        {
            _logger.LogInformation("[WEBHOOK RAW] " + body.ToString());

            await _processWebhookUseCase.ExecuteAsync(body);

            return Ok();
        }
    }

}

