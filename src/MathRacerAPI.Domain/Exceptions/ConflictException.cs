using System;

namespace MathRacerAPI.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando hay un conflicto de estado en la operación
/// Por ejemplo: intentar comprar un producto que ya se posee
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}