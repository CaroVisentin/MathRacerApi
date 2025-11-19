using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Payment;
using MercadoPago.Client.Preference;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly GetCoinPackageUseCase _getCoinPackageUseCase;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            GetCoinPackageUseCase getCoinPackageUseCase,
            ILogger<PaymentsController> logger)
        {
            _getCoinPackageUseCase = getCoinPackageUseCase;
            _logger = logger;
        }

       [SwaggerOperation(
            Summary = "Crea una preferencia de pago",
            Description = "Crea una preferencia de pago en MercadoPago para un paquete de monedas específico."
        )]
        [SwaggerResponse(200, "Preferencia de pago creada exitosamente")]
        [SwaggerResponse(404, "No se encontró el paquete de monedas")]
        [SwaggerResponse(500, "Error interno del servidor")]
    
        
        [HttpPost("create-preference")]
        public async Task<IActionResult> CreatePreference([FromBody] CreatePreferenceRequest req)
        {
            if (req == null)
            {
                return BadRequest(new { message = "Empty payload" });
            }

            if (req.PlayerId <= 0 || req.CoinPackageId <= 0)
            {
                return BadRequest(new { message = "PlayerId and CoinPackageId must be valid" });
            }

            if (string.IsNullOrWhiteSpace(req.SuccessUrl) ||
                string.IsNullOrWhiteSpace(req.FailureUrl) ||
                string.IsNullOrWhiteSpace(req.PendingUrl))
            {
                return BadRequest(new { message = "You must provide all return URLs." });
            }

            try
            {
                var coinPackage = await _getCoinPackageUseCase.ExecuteAsync(req.CoinPackageId);
                if (coinPackage == null)
                {
                    _logger.LogWarning("[PAYMENTS] Coin package {CoinPackageId} not found", req.CoinPackageId);
                    return NotFound(new { message = "Coin package not found." });
                }

                var backendUrl = Environment.GetEnvironmentVariable("BACKEND_URL");
                if (string.IsNullOrWhiteSpace(backendUrl))
                {
                    _logger.LogError("[PAYMENTS] BACKEND_URL environment variable not configured");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Could not generate preference: invalid configuration." });
                }

                var preferenceRequest = new PreferenceRequest
                {
                    Items = new List<PreferenceItemRequest>
                    {
                        new PreferenceItemRequest
                        {
                            Title = coinPackage.Description,
                            Quantity = 1,
                            UnitPrice = (decimal)coinPackage.Price
                        }
                    },
                    AutoReturn = "approved",
                    BackUrls = new PreferenceBackUrlsRequest
                    {
                        Success = req.SuccessUrl,
                        Failure = req.FailureUrl,
                        Pending = req.PendingUrl
                    },
                    ExternalReference = $"{req.PlayerId}_{req.CoinPackageId}",
                    NotificationUrl = $"{backendUrl.TrimEnd('/')}/api/webhook"
                };

                var client = new PreferenceClient();
                try
                {
                    var preference = await client.CreateAsync(preferenceRequest);
                    return Ok(new { preferenceId = preference.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[PAYMENTS] Error while creating preference in MercadoPago");
                    return StatusCode(StatusCodes.Status502BadGateway, new { message = "Mercado Pago no respondió correctamente. Intenta de nuevo en unos minutos." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENTS] Unexpected error while creating preference");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "No se pudo generar la preferencia de pago." });
            }
        }
    }

}


