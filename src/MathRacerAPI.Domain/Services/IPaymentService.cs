using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Services
{
    public interface IPaymentService
    {

        Task<string?> CreatePreferenceAsync(string successUrl, string pendingUrl, string failureUrl, CoinPackage coinPackage, int playerId);
    }
}
