using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Infrastructure.Repositories;
using MathRacerAPI.Infrastructure.Providers;
using MathRacerAPI.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
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
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registrar casos de uso (modo offline)
        services.AddScoped<GetApiInfoUseCase>();
        services.AddScoped<GetHealthStatusUseCase>();
        services.AddScoped<CreateGameUseCase>();
        services.AddScoped<JoinGameUseCase>();
        services.AddScoped<GetNextQuestionUseCase>();
        services.AddScoped<SubmitAnswerUseCase>();

        // Registrar casos de uso (modo online)
        services.AddScoped<FindMatchUseCase>();
        services.AddScoped<ProcessOnlineAnswerUseCase>();
        services.AddScoped<GetNextOnlineQuestionUseCase>();

        // Registrar repositorios
        services.AddScoped<IGameRepository, InMemoryGameRepository>();

        // Registrar servicios de dominio (lógica compartida)
        services.AddScoped<IGameLogicService, GameLogicService>();
        services.AddScoped<IPowerUpService, PowerUpService>();

        // Registrar proveedores
        services.AddSingleton<MathRacerAPI.Domain.Providers.IQuestionProvider, MathRacerAPI.Infrastructure.Providers.QuestionProvider>();

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

            // Incluir comentarios XML
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
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