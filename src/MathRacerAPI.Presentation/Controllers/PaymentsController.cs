using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Payment;
using MercadoPago.Client.Preference;
using Microsoft.AspNetCore.Mvc;

namespace MathRacerAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly GetCoinPackageUseCase _getCoinPackageUseCase;
        public PaymentsController(GetCoinPackageUseCase getCoinPackageUseCase)
        {
            _getCoinPackageUseCase = getCoinPackageUseCase;
        }

        [HttpPost("create-preference")]
        public async Task<IActionResult> CreatePreference(CreatePreferenceRequest req)
        {
            var coinPackage = await _getCoinPackageUseCase.ExecuteAsync(req.CoinPackageId);

            if (coinPackage == null)
                return NotFound("CoinPackage not found");

            var client = new PreferenceClient();

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
                NotificationUrl = Environment.GetEnvironmentVariable("BACKEND_URL") +"/api/webhook"    
            };

            var preference = await client.CreateAsync(preferenceRequest);

            return Ok(new { preferenceId = preference.Id });
        }
    }

}


