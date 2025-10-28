using MathRacerAPI.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Repositories
{
    public interface IPlayerRepository
    {
        Task<PlayerProfile?> GetByIdAsync(int id);
        Task<PlayerProfile?> GetByEmailAsync(string email);
        Task<PlayerProfile?> GetByUidAsync(string uid);
        Task<PlayerProfile> AddAsync(PlayerProfile playerProfile);
    }
}
