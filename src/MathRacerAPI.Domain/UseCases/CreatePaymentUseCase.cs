using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;

namespace MathRacerAPI.Domain.UseCases
{
    public class CreatePaymentUseCase
    {
        private readonly IPaymentService _paymentService ;

        public CreatePaymentUseCase(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        public async Task<PaymentResponse> ExecuteAsync(CoinPackage coinPackage, int playerId, string successUrl, string pendingUrl, string failureUrl)
        {

            var preferenceResponse = await _paymentService.CreatePreferenceAsync(
                successUrl,
                pendingUrl,
                failureUrl,
                coinPackage,
                playerId);

            if (preferenceResponse == null)
            {
                throw new Exception("Failed to create payment preference.");
            }

            return preferenceResponse;
        }
    }
    }
