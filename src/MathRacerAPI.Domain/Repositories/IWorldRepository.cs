using MathRacerAPI.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories
{
    public interface IWorldRepository
    {
        Task<World?> GetByIdAsync(int id);
    }
}
