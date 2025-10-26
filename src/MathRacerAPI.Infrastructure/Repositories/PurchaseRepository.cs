using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Configuration;
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
    }
}

