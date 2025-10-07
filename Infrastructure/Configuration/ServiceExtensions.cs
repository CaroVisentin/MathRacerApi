using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Infrastructure.Repositories;
using MathRacerAPI.Infrastructure.Providers;

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
        // Registrar casos de uso
        services.AddScoped<GetApiInfoUseCase>();
        services.AddScoped<GetHealthStatusUseCase>();
        services.AddScoped<CreateGameUseCase>();
        services.AddScoped<JoinGameUseCase>();
        services.AddScoped<GetNextQuestionUseCase>();
        services.AddScoped<SubmitAnswerUseCase>();
        services.AddScoped<GenerateEquationUseCase>();

        // Registrar repositorios
        services.AddScoped<IGameRepository, InMemoryGameRepository>();

        services.AddSingleton(new QuestionProvider("Infrastructure/Providers/ecuaciones.json"));

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