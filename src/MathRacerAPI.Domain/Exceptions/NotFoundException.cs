using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando un recurso no es encontrado.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public NotFoundException(string resourceName, object key) 
        : base($"{resourceName} con ID '{key}' no fue encontrado.")
    {
    }
}
