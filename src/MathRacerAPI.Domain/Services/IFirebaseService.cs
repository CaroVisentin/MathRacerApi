using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Services
{
    public interface IFirebaseService
    {
        Task<string?> ValidateIdTokenAsync(string idToken);
    }
}
