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
        services.AddScoped<GetPlayerByIdUseCase>();
        services.AddScoped<GetWorldsUseCase>();
        
        // Registrar casos de uso de Players
        services.AddScoped<CreatePlayerUseCase>();

        // Registrar casos de uso (modo online)
        services.AddScoped<FindMatchUseCase>();
        services.AddScoped<ProcessOnlineAnswerUseCase>();
        services.AddScoped<GetNextOnlineQuestionUseCase>();

        // Registrar repositorios
        services.AddScoped<IGameRepository, InMemoryGameRepository>();
        services.AddScoped<ILevelRepository, LevelRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IWorldRepository, WorldRepository>();  

        // Cargar el archivo .env correspondiente al entorno
        DotNetEnv.Env.Load($".env.{environment.EnvironmentName.ToLower()}");

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