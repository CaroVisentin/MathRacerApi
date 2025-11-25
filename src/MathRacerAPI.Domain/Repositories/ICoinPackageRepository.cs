using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Domain.Repositories
{
    public interface ICoinPackageRepository
    {
            Task<CoinPackage?> GetByIdAsync(int id);
            Task<List<CoinPackage>> GetAllAsync();
       
    }

}
