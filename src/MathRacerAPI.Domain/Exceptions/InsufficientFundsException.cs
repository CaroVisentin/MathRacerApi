using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando el jugador no tiene suficientes monedas para realizar una compra
/// </summary>
public class InsufficientFundsException : BusinessException
{
    public InsufficientFundsException(string message) : base(message)
    {
    }

    public InsufficientFundsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}