using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Repositories
{

    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly MathiRacerDbContext _context;

        public PurchaseRepository(MathiRacerDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Purchase purchase)
        {
            // Map Purchase (Domain Model) to PurchaseEntity (Infrastructure Entity)
            var purchaseEntity = new PurchaseEntity
            {
                Id = purchase.Id,
                PlayerId = purchase.PlayerId,
                CoinPackageId = purchase.CoinPackageId,
                TotalAmount = purchase.TotalAmount,
                Date = purchase.Date,
                PaymentMethodId = purchase.PaymentMethodId,
                PaymentId = purchase.PaymentId
            };

            await _context.Purchases.AddAsync(purchaseEntity);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();


        public async Task<bool> ExistsByPaymentIdAsync(string paymentId)
        {
            return await _context.Purchases
                .AnyAsync(p => p.PaymentId == paymentId);
        }

    }
}

