using System.Net;
using System.Text.Json;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Presentation.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones en la aplicación.
/// Intercepta todas las excepciones no controladas y genera respuestas HTTP consistentes.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Invoca el siguiente middleware en el pipeline y captura excepciones
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continuar con el siguiente middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            // Capturar y manejar cualquier excepción
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maneja la excepción y genera una respuesta HTTP apropiada
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Ocurrió un error interno en el servidor.";
        object? details = null;

        // Manejo especial para errores de token Firebase
        if (exception.Message.Contains("Incorrect number of segments in ID token") ||
            exception.Message.Contains("FirebaseAuthException") ||
            exception.Message.Contains("ID token") ||
            exception.Message.Contains("token is invalid"))
        {
            statusCode = HttpStatusCode.Unauthorized;
            message = "El token de autenticación es inválido o no fue enviado.";
            _logger.LogWarning(exception, "Token inválido: {Message}", exception.Message);
        }

        // Determinar el código de estado HTTP según el tipo de excepción
        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                _logger.LogWarning(notFoundEx, "Recurso no encontrado: {Message}", notFoundEx.Message);
                break;

            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                message = conflictEx.Message;
                _logger.LogWarning(conflictEx, "Conflicto de estado: {Message}", conflictEx.Message);
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                message = validationEx.Message;
                details = validationEx.Errors;
                _logger.LogWarning(validationEx, "Error de validación: {Message}", validationEx.Message);
                break;

            case BusinessException businessEx:
                statusCode = HttpStatusCode.BadRequest;
                message = businessEx.Message;
                _logger.LogWarning(businessEx, "Error de lógica de negocio: {Message}", businessEx.Message);
                break;

            case UnauthorizedAccessException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                message = "No autorizado para realizar esta acción.";
                _logger.LogWarning(unauthorizedEx, "Acceso no autorizado: {Message}", unauthorizedEx.Message);
                break;

            default:
                // Error no esperado - log completo con stack trace
                _logger.LogError(exception, 
                    "Error no controlado: {Message}\nStackTrace: {StackTrace}", 
                    exception.Message, 
                    exception.StackTrace);
                break;
        }

        // Configurar la respuesta HTTP
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Construir el objeto de respuesta

        object response;
        if (_environment.IsDevelopment())
        {
            response = new
            {
                StatusCode = (int)statusCode,
                Message = message,
                Details = details,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException != null ? exception.InnerException.ToString() : null
            };
        }
        else
        {
            response = new
            {
                StatusCode = (int)statusCode,
                Message = message,
                Details = details
            };
        }

        // Serializar y enviar la respuesta
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment() // Formatear en desarrollo
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}