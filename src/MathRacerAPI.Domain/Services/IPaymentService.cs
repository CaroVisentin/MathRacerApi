using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePreferenceAsync(string successUrl, string pendingUrl, string failureUrl, CoinPackage coinPackage, int playerId);

        Task<PaymentInfo> ProcessWebhookNotificationAsync(JsonElement json);

    }
}
