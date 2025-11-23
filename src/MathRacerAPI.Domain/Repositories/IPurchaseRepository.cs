using MathRacerAPI.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories
{
    public interface IPurchaseRepository
    {
        Task AddAsync(Purchase purchase);
        Task SaveChangesAsync();
        public Task<bool> ExistsByPaymentIdAsync(string paymentId);

    }
}
