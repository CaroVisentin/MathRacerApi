using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Infrastructure.Repositories;
using MathRacerAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace MathRacerAPI.Infrastructure.Configuration;

/// <summary>
/// Extensiones para configurar los servicios de la aplicación
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Configura los servicios de la aplicación
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Registrar casos de uso (modo offline)
        services.AddScoped<GetApiInfoUseCase>();
        services.AddScoped<GetHealthStatusUseCase>();
        services.AddScoped<CreateGameUseCase>();
        services.AddScoped<JoinGameUseCase>();
        services.AddScoped<GetNextQuestionUseCase>();
        services.AddScoped<SubmitAnswerUseCase>();
        services.AddScoped<GetQuestionsUseCase>();

        // Registrar casos de uso de Players
        services.AddScoped<CreatePlayerUseCase>();
        services.AddScoped<GetPlayerByIdUseCase>();

        // Registrar casos de uso de Worlds
        services.AddScoped<GetWorldsUseCase>();
        
        // Registrar casos de uso de Players
        services.AddScoped<RegisterPlayerUseCase>();
        services.AddScoped<LoginPlayerUseCase>();
        services.AddScoped<GoogleAuthUseCase>();

        // Registrar casos de uso de Levels
        services.AddScoped<GetWorldLevelsUseCase>();

        // Registrar casos de uso (modo online)
        services.AddScoped<FindMatchUseCase>();
        services.AddScoped<ProcessOnlineAnswerUseCase>();
        services.AddScoped<GetNextOnlineQuestionUseCase>();

        // Registrar repositorios
        services.AddScoped<IGameRepository, InMemoryGameRepository>();
        services.AddScoped<ILevelRepository, LevelRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IWorldRepository, WorldRepository>();  

    // Registrar servicio de Firebase
    services.AddScoped<IFirebaseService, FirebaseService>();

    // Cargar el archivo .env fijo para todos los entornos
    DotNetEnv.Env.Load(".env");

        // Leer la cadena de conexión
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");

        services.AddDbContext<MathiRacerDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Registrar servicios de dominio (lógica compartida)
        services.AddScoped<IGameLogicService, GameLogicService>();
        services.AddScoped<IPowerUpService, PowerUpService>();

        // Configurar SignalR
        services.AddSignalR();

        return services;
    }

    /// <summary>
    /// Configura Swagger/OpenAPI
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
            { 
                Title = "MathRacer API", 
                Version = "v1",
                Description = "API para el juego MathRacer - Competencias matemáticas en tiempo real",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "MathRacer Team"
                }
            });

            // Incluir comentarios XML del proyecto Presentation (Controladores)
            var presentationXmlFile = "MathRacerAPI.Presentation.xml";
            var presentationXmlPath = Path.Combine(AppContext.BaseDirectory, presentationXmlFile);
            if (File.Exists(presentationXmlPath))
            {
                c.IncludeXmlComments(presentationXmlPath);
            }

            // Incluir comentarios XML del proyecto Domain (Modelos y DTOs)
            var domainXmlFile = "MathRacerAPI.Domain.xml";
            var domainXmlPath = Path.Combine(AppContext.BaseDirectory, domainXmlFile);
            if (File.Exists(domainXmlPath))
            {
                c.IncludeXmlComments(domainXmlPath);
            }

            // Incluir comentarios XML del proyecto Infrastructure
            var infrastructureXmlFile = "MathRacerAPI.Infrastructure.xml";
            var infrastructureXmlPath = Path.Combine(AppContext.BaseDirectory, infrastructureXmlFile);
            if (File.Exists(infrastructureXmlPath))
            {
                c.IncludeXmlComments(infrastructureXmlPath);
            }

            c.OperationFilter<ErrorResponseExamplesOperationFilter>();

        });

        return services;
    }

    /// <summary>
    /// Configura health checks
    /// </summary>
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }
}

/// <summary>
/// Filtro para personalizar los ejemplos de respuestas de error en Swagger
/// </summary>
public class ErrorResponseExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Ejemplos por código de error - TODOS con el mismo formato
        var errorExamples = new Dictionary<string, OpenApiObject>
        {
            ["400"] = new OpenApiObject
            {
                ["statusCode"] = new OpenApiInteger(400),
                ["message"] = new OpenApiString("Error de validación o regla de negocio"),
                ["details"] = new OpenApiNull(),
                ["stackTrace"] = new OpenApiString("StackTrace disponible solo en modo desarrollo"),
                ["innerException"] = new OpenApiNull()
            },
            ["404"] = new OpenApiObject
            {
                ["statusCode"] = new OpenApiInteger(404),
                ["message"] = new OpenApiString("Recurso no encontrado"),
                ["details"] = new OpenApiNull(),
                ["stackTrace"] = new OpenApiString("StackTrace disponible solo en modo desarrollo"),
                ["innerException"] = new OpenApiNull()
            },
            ["500"] = new OpenApiObject
            {
                ["statusCode"] = new OpenApiInteger(500),
                ["message"] = new OpenApiString("Ocurrió un error interno en el servidor."),
                ["details"] = new OpenApiNull(),
                ["stackTrace"] = new OpenApiString("StackTrace disponible solo en modo desarrollo"),
                ["innerException"] = new OpenApiNull()
            }
        };

        // Iterar sobre TODAS las respuestas
        foreach (var response in operation.Responses)
        {
            // Solo aplicar a códigos de error (4xx, 5xx)
            if (response.Key.StartsWith("4") || response.Key.StartsWith("5"))
            {
                // Si la respuesta tiene content
                if (response.Value.Content != null && response.Value.Content.Any())
                {
                    foreach (var content in response.Value.Content.Values)
                    {
                        // Aplicar ejemplo
                        if (errorExamples.TryGetValue(response.Key, out var example))
                        {
                            content.Example = example;
                        }
                    }
                }
                else
                {
                    // Si NO tiene content, crearlo (esto debería solucionar el problema del 500)
                    response.Value.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = errorExamples.TryGetValue(response.Key, out var example) 
                                ? example 
                                : null
                        }
                    };
                }
            }
        }
    }
}