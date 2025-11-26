using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Services;
using MercadoPago.Client.MerchantOrder;
using MercadoPago.Client.Payment;
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
                var response = new PaymentResponse { PreferenceId = preference.Id, InitPoint = preference.InitPoint };
                return response;

            }
            catch
            {
                _logger.LogError("[PAYMENT] Error creating preference");
                return null;
            }
        }

        public async Task<PaymentInfo> ProcessWebhookNotificationAsync(JsonElement json)
        {
            try
            {
                string topic = GetString(json, "topic") ?? GetString(json, "type");

                string idStr = null;

                if (json.TryGetProperty("data", out JsonElement dataElem))
                {
                    idStr = GetString(dataElem, "id");
                }

                if (string.IsNullOrEmpty(idStr)) idStr = GetString(json, "id");
                if (string.IsNullOrEmpty(idStr)) idStr = GetString(json, "resource");

                if (!string.IsNullOrEmpty(idStr) && !long.TryParse(idStr, out _))
                {
                    idStr = idStr.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                }

                if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(idStr)) return null;

                if (topic == "payment")
                {
                    return await GetFromPaymentId(idStr);
                }
                else if (topic == "merchant_order")
                {
                    return await GetFromMerchantOrder(idStr);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENT-SERVICE] Error parseando webhook JSON");
                return null;
            }
        }


        private async Task<PaymentInfo> GetFromPaymentId(string idStr)
        {
            if (!long.TryParse(idStr, out long paymentId)) return null;

            var client = new PaymentClient();
            var payment = await client.GetAsync(paymentId);

            if (payment == null) return null;

            return new PaymentInfo
            {
                PaymentId = payment.Id.ToString(),
                Status = payment.Status,
                ExternalReference = payment.ExternalReference
            };
        }

        private async Task<PaymentInfo> GetFromMerchantOrder(string idStr)
        {
            if (!long.TryParse(idStr, out long orderId)) return null;

            var client = new MerchantOrderClient();
            var order = await client.GetAsync(orderId);

            var approvedPayment = order.Payments?.FirstOrDefault(p => p.Status == "approved");

            if (approvedPayment == null) return null;

            // Usamos el ID del pago aprobado para obtener el detalle completo
            return await GetFromPaymentId(approvedPayment.Id.ToString());
        }
    

        private string? GetString(JsonElement element, string propName)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propName, out JsonElement value))
            {
                return value.ToString();
            }
            return null;
        }

    }

}


