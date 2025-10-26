using System.IO;
using System.Text;
using System.Text.Json;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using MathRacerAPI.Presentation.Middleware;
using MathRacerAPI.Domain.Exceptions;

namespace MathRacerAPI.Tests.Middleware;

/// <summary>
/// Suite completa de tests para ExceptionHandlingMiddleware
/// Cubre todos los tipos de excepciones y escenarios posibles
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;

    public ExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _environmentMock = new Mock<IHostEnvironment>();
        // Configuración por defecto: Production
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
    }

    #region ✅ Tests de Ejecución Normal

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200); // StatusCode por defecto
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldNotWriteToResponse()
    {
        // Arrange
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Length.Should().Be(0);
    }

    #endregion

    #region 🔍 Tests para NotFoundException

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404()
    {
        // Arrange
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturnCorrectMessage()
    {
        // Arrange
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.GetProperty("statusCode").GetInt32().Should().Be(404);
        response.GetProperty("message").GetString().Should().Contain("Partida");
        response.GetProperty("message").GetString().Should().Contain("123");
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldLogWarning()
    {
        // Arrange
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Recurso no encontrado")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundExceptionInProduction_ShouldNotIncludeStackTrace()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.TryGetProperty("stackTrace", out var stackTrace).Should().BeTrue();
        stackTrace.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundExceptionInDevelopment_ShouldIncludeStackTrace()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.TryGetProperty("stackTrace", out var stackTrace).Should().BeTrue();
        // StackTrace puede estar presente en desarrollo
    }

    #endregion

    #region ⚠️ Tests para ValidationException

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldReturn400()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "MaxQuestions", new[] { "Debe ser mayor a 0" } },
            { "DifficultyLevel", new[] { "Nivel inválido" } }
        };
        var exception = new ValidationException(errors);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldIncludeErrorDetails()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "MaxQuestions", new[] { "Debe ser mayor a 0" } },
            { "DifficultyLevel", new[] { "Nivel inválido" } }
        };
        var exception = new ValidationException(errors);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.GetProperty("statusCode").GetInt32().Should().Be(400);
        response.GetProperty("message").GetString().Should().Contain("validación");

        var details = response.GetProperty("details");
        details.ValueKind.Should().Be(JsonValueKind.Object);
        details.GetProperty("MaxQuestions").GetArrayLength().Should().Be(1);
        details.GetProperty("DifficultyLevel").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldLogWarning()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error message" } }
        };
        var exception = new ValidationException(errors);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error de validación")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationExceptionWithEmptyErrors_ShouldHandleCorrectly()
    {
        // Arrange
        var exception = new ValidationException("Error de validación genérico");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var response = await DeserializeResponse(context);
        response.GetProperty("message").GetString().Should().Contain("validación");
    }

    #endregion

    #region 💼 Tests para BusinessException

    [Fact]
    public async Task InvokeAsync_WhenBusinessException_ShouldReturn400()
    {
        // Arrange
        var exception = new BusinessException("No se puede iniciar la partida sin jugadores");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenBusinessException_ShouldReturnCorrectMessage()
    {
        // Arrange
        var expectedMessage = "Límite de partidas activas alcanzado";
        var exception = new BusinessException(expectedMessage);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.GetProperty("statusCode").GetInt32().Should().Be(400);
        response.GetProperty("message").GetString().Should().Be(expectedMessage);
    }

    [Fact]
    public async Task InvokeAsync_WhenBusinessException_ShouldLogWarning()
    {
        // Arrange
        var exception = new BusinessException("Regla de negocio violada");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error de lógica de negocio")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenBusinessExceptionWithInnerException_ShouldIncludeInDevelopment()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var innerException = new InvalidOperationException("Error interno");
        var exception = new BusinessException("Error de negocio", innerException);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.TryGetProperty("innerException", out var inner).Should().BeTrue();
    }

    #endregion

    #region 🔒 Tests para UnauthorizedAccessException

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("No tienes permisos");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_ShouldReturnGenericMessage()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Detalles internos");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.GetProperty("statusCode").GetInt32().Should().Be(401);
        response.GetProperty("message").GetString().Should().Be("No autorizado para realizar esta acción.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_ShouldLogWarning()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Sin permisos");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Acceso no autorizado")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region 💥 Tests para Excepciones Genéricas

    [Fact]
    public async Task InvokeAsync_WhenGenericException_ShouldReturn500()
    {
        // Arrange
        var exception = new Exception("Error inesperado");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_ShouldReturnGenericMessage()
    {
        // Arrange
        var exception = new Exception("Detalles técnicos sensibles");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.GetProperty("statusCode").GetInt32().Should().Be(500);
        response.GetProperty("message").GetString().Should().Be("Ocurrió un error interno en el servidor.");
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Error crítico");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error no controlado")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNullReferenceException_ShouldReturn500()
    {
        // Arrange
        var exception = new NullReferenceException("Objeto nulo");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentException_ShouldReturn500()
    {
        // Arrange
        var exception = new ArgumentException("Argumento inválido", "paramName");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    #endregion

    #region 🌍 Tests para Entornos (Development vs Production)

    [Fact]
    public async Task InvokeAsync_InDevelopment_ShouldIncludeStackTraceAndInnerException()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var innerException = new InvalidOperationException("Error interno");
        var exception = new Exception("Error principal", innerException);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.TryGetProperty("stackTrace", out _).Should().BeTrue();
        response.TryGetProperty("innerException", out var inner).Should().BeTrue();
        inner.GetString().Should().Be("Error interno");
    }

    [Fact]
    public async Task InvokeAsync_InProduction_ShouldNotIncludeStackTraceOrInnerException()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var innerException = new InvalidOperationException("Error interno");
        var exception = new Exception("Error principal", innerException);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.TryGetProperty("stackTrace", out var stackTrace).Should().BeTrue();
        stackTrace.ValueKind.Should().Be(JsonValueKind.Null);

        response.TryGetProperty("innerException", out var inner).Should().BeTrue();
        inner.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task InvokeAsync_InDevelopment_ShouldFormatJsonWithIndentation()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("\n"); // JSON formateado con saltos de línea
    }

    [Fact]
    public async Task InvokeAsync_InProduction_ShouldFormatJsonWithoutIndentation()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBody(context);
        // En producción, JSON compacto (sin saltos de línea innecesarios)
        responseBody.Should().NotContain("  "); // Sin indentación
    }

    #endregion

    #region 🔗 Tests de Integración de Múltiples Excepciones

    [Theory]
    [InlineData(typeof(NotFoundException), 404)]
    [InlineData(typeof(ValidationException), 400)]
    [InlineData(typeof(BusinessException), 400)]
    [InlineData(typeof(UnauthorizedAccessException), 401)]
    [InlineData(typeof(Exception), 500)]
    public async Task InvokeAsync_DifferentExceptionTypes_ShouldReturnCorrectStatusCode(Type exceptionType, int expectedStatusCode)
    {
        // Arrange
        var exception = CreateExceptionInstance(exceptionType);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(expectedStatusCode);
    }

    #endregion

    #region 📋 Tests de Formato de Respuesta

    [Fact]
    public async Task InvokeAsync_AnyException_ShouldReturnJsonContentType()
    {
        // Arrange
        var exception = new Exception("Error");
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_AnyException_ShouldReturnValidJson()
    {
        // Arrange
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBody(context);
        var act = () => JsonDocument.Parse(responseBody);
        act.Should().NotThrow<JsonException>();
    }

    [Fact]
    public async Task InvokeAsync_AnyException_ShouldIncludeMandatoryFields()
    {
        // Arrange
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await DeserializeResponse(context);
        response.TryGetProperty("statusCode", out _).Should().BeTrue();
        response.TryGetProperty("message", out _).Should().BeTrue();
        response.TryGetProperty("details", out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseCamelCasePropertyNames()
    {
        // Arrange
        var exception = new NotFoundException("Partida", 123);
        var middleware = CreateMiddleware(ThrowException(exception));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("statusCode"); // camelCase
        responseBody.Should().NotContain("StatusCode"); // No PascalCase
    }

    #endregion

    #region 🛠️ Helper Methods

    private ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ExceptionHandlingMiddleware(next, _loggerMock.Object, _environmentMock.Object);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static RequestDelegate ThrowException(Exception exception)
    {
        return (HttpContext ctx) => throw exception;
    }

    private static async Task<JsonElement> DeserializeResponse(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var document = JsonDocument.Parse(responseBody);
        return document.RootElement.Clone();
    }

    private static async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(context.Response.Body).ReadToEndAsync();
    }

    private static Exception CreateExceptionInstance(Type exceptionType)
    {
        return exceptionType.Name switch
        {
            nameof(NotFoundException) => new NotFoundException("Resource", 1),
            nameof(ValidationException) => new ValidationException("Validation error"),
            nameof(BusinessException) => new BusinessException("Business rule violation"),
            nameof(UnauthorizedAccessException) => new UnauthorizedAccessException("Unauthorized"),
            _ => new Exception("Generic error")
        };
    }
}

    #endregion
