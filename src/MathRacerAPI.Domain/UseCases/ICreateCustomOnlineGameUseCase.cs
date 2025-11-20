using MathRacerAPI.Domain.Models;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Interfaz para el caso de uso de crear una partida multijugador personalizada
/// </summary>
public interface ICreateCustomOnlineGameUseCase
{
    /// <summary>
    /// Crea una partida personalizada SIN agregar jugadores aún.
    /// El creador se unirá posteriormente mediante JoinCreatedGameUseCase con su ConnectionId real.
    /// </summary>
    Task<Game> ExecuteAsync(
        string firebaseUid,
        string gameName,
        bool isPrivate,
        string? password,
        string difficulty,
        string expectedResult);
}