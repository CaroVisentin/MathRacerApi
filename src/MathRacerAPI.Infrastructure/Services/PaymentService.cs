using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Services;
using MercadoPago.Client.Preference;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MathRacerAPI.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        public PaymentService(ILogger<PaymentService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        async Task<PaymentResponse> IPaymentService.CreatePreferenceAsync(string successUrl, string pendingUrl, string failureUrl, CoinPackage coinPackage, int playerId)
        {

            var backendUrl = _configuration.GetValue<string>("Payment:BackUrl");

            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                throw new InvalidOperationException("Payment:BackUrl no está configurado.");
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
                    Success = successUrl,
                    Failure = failureUrl,
                    Pending = pendingUrl
                },
                ExternalReference = $"{playerId}_{coinPackage.Id}",
                NotificationUrl = $"{backendUrl.TrimEnd('/')}/api/webhook"
            };

            var client = new PreferenceClient();
            try
            {
                var preference = await client.CreateAsync(preferenceRequest);
                var response = new PaymentResponse {PreferenceId = preference.Id, InitPoint = preference.InitPoint };
                return response;

            }
            catch
            {
                _logger.LogError("[PAYMENT] Error creating preference");
                return null;
            }
        }
    }
}
