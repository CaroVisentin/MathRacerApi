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
using System.Linq;


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
        // Registrar casos de uso de información general
        services.AddScoped<GetApiInfoUseCase>();
        services.AddScoped<GetHealthStatusUseCase>();

        // Registrar casos de uso de Players
        services.AddScoped<CreatePlayerUseCase>();
        services.AddScoped<GetPlayerByIdUseCase>();
        services.AddScoped<GetPlayerByEmailUseCase>();
        services.AddScoped<GetPlayerEnergyStatusUseCase>();

        // Registrar casos de uso de Worlds
        services.AddScoped<GetWorldsUseCase>();
        
        // Registrar casos de uso de Players
        services.AddScoped<RegisterPlayerUseCase>();
        services.AddScoped<LoginPlayerUseCase>();
        services.AddScoped<GoogleAuthUseCase>();
        services.AddScoped<DeletePlayerUseCase>();

        // Registrar casos de uso de Levels
        services.AddScoped<GetWorldLevelsUseCase>();

        // Registrar casos de uso de Ecuaciones
        services.AddScoped<GetQuestionsUseCase>();

        // Registrar casos de uso de Wildcards
        services.AddScoped<GetPlayerWildcardsUseCase>();
        services.AddScoped<GetStoreWildcardsUseCase>();
        services.AddScoped<PurchaseWildcardUseCase>();

        // Registrar casos de uso de modo individual
        services.AddScoped<StartSoloGameUseCase>();
        services.AddScoped<GetSoloGameStatusUseCase>();
        services.AddScoped<SubmitSoloAnswerUseCase>();

        // Registrar casos de uso de modo online 
        services.AddScoped<FindMatchUseCase>();
        services.AddScoped<FindMatchWithMatchmakingUseCase>();
        services.AddScoped<ProcessOnlineAnswerUseCase>();
        services.AddScoped<GetNextOnlineQuestionUseCase>();
        services.AddScoped<GrantLevelRewardUseCase>();
        services.AddScoped<UseWildcardUseCase>();
        services.AddScoped<JoinCreatedGameUseCase>();

        services.AddScoped<ICreateCustomOnlineGameUseCase, CreateCustomOnlineGameUseCase>();
        services.AddScoped<GetAvailableGamesUseCase>();

        // Registrar casos de uso de Chests
        services.AddScoped<OpenTutorialChestUseCase>();
        services.AddScoped<OpenRandomChestUseCase>();
        services.AddScoped<PurchaseRandomChestUseCase>();

        // Registrar casos de uso de Store
        services.AddScoped<GetStoreCarsUseCase>();
        services.AddScoped<GetStoreCharactersUseCase>();
        services.AddScoped<GetStoreBackgroundsUseCase>();
        services.AddScoped<PurchaseStoreItemUseCase>();
            
        // Registrar casos de uso de Garage
        services.AddScoped<GetPlayerGarageItemsUseCase>();
        services.AddScoped<ActivatePlayerItemUseCase>();
      
        // Registrar casos de uso de Ranking
        services.AddScoped<IGetPlayerRankingUseCase, GetPlayerRankingUseCase>();

        // Registrar casos de uso de amistad
        services.AddScoped<SendFriendRequestUseCase>();
        services.AddScoped<AcceptFriendRequestUseCase>();
        services.AddScoped<RejectFriendRequestUseCase>();
        services.AddScoped<GetFriendsUseCase>();
        services.AddScoped<DeleteFriendUseCase>();
        services.AddScoped<GetPendingFriendRequestsUseCase>();

        // Registrar casos de uso de tienda/energía
        services.AddScoped<PurchaseEnergyUseCase>();
        services.AddScoped<GetEnergyStoreInfoUseCase>();

        // Registrar casos de uso de pagos/paquetes de monedas
        services.AddScoped<GetCoinPackageUseCase>();
        services.AddScoped<GetAllCoinPackagesUseCase>();
        services.AddScoped<PurchaseExistsByPaymentIdUseCase>();
        services.AddScoped<PersistPurchaseUseCase>();
        services.AddScoped<AddCoinsToPlayerUseCase>();

        // Registrar casos de uso de modo infinito
        services.AddScoped<StartInfiniteGameUseCase>();
        services.AddScoped<SubmitInfiniteAnswerUseCase>();
        services.AddScoped<LoadNextBatchUseCase>();
        services.AddScoped<GetInfiniteGameStatusUseCase>();
        services.AddScoped<AbandonInfiniteGameUseCase>();

        services.AddScoped<CreatePaymentUseCase>();

        // Registrar casos de uso de invitaciones de juego
        services.AddScoped<SendGameInvitationUseCase>();
        services.AddScoped<GetGameInvitationsUseCase>();
        services.AddScoped<RespondGameInvitationUseCase>();

        // Registrar repositorios
        services.AddScoped<ILevelRepository, LevelRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IWorldRepository, WorldRepository>();
        services.AddScoped<IGarageRepository, GarageRepository>();  
        services.AddScoped<IRankingRepository, RankingRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IEnergyRepository, EnergyRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ICoinPackageRepository, CoinPackageRepository>();
        services.AddScoped<IGameRepository, InMemoryGameRepository>();
        services.AddSingleton<ISoloGameRepository, InMemorySoloGameRepository>();
        services.AddSingleton<IInfiniteGameRepository, InMemoryInfiniteGameRepository>();
        services.AddScoped<IGameInvitationRepository>(provider =>
        {
            var context = provider.GetRequiredService<MathiRacerDbContext>();
            var gameRepository = provider.GetRequiredService<IGameRepository>();
            return new GameInvitationRepository(context, gameRepository);
        });


        // Registrar repositorios de amistad
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<IChestRepository, ChestRepository>();
        services.AddScoped<IWildcardRepository, WildcardRepository>();
       services.AddScoped<IPurchaseRepository, PurchaseRepository>();


        // Registrar servicio de Firebase
        services.AddSingleton<IFirebaseService, FirebaseService>();
        services.AddSingleton<IPaymentService, PaymentService> ();

    // Cargar el archivo .env fijo para todos los entornos
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
                Version = "1.0.0",
                Description = "API para el juego MathRacer - Competencias matemáticas en tiempo real",
                License = new Microsoft.OpenApi.Models.OpenApiLicense
                {
                    Name = "MIT License"
                }
            });

            // Habilitar anotaciones Swagger
            c.EnableAnnotations();

            // Configurar para OpenAPI 3.0
            c.DescribeAllParametersInCamelCase();
            
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
        if (operation?.Responses == null)
            return;

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
            ["401"] = new OpenApiObject
            {
                ["statusCode"] = new OpenApiInteger(401),
                ["message"] = new OpenApiString("No autorizado. Token inválido o faltante."),
                ["details"] = new OpenApiNull(),
                ["stackTrace"] = new OpenApiString("StackTrace disponible solo en modo desarrollo"),
                ["innerException"] = new OpenApiNull()
            },
            ["403"] = new OpenApiObject
            {
                ["statusCode"] = new OpenApiInteger(403),
                ["message"] = new OpenApiString("Prohibido. No tienes permiso para realizar esta acción."),
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
        foreach (var response in operation.Responses.ToList()) // ToList() para evitar modificar durante la iteración
        {
            // Solo aplicar a códigos de error (4xx, 5xx)
            if (string.IsNullOrEmpty(response.Key) || 
                (!response.Key.StartsWith("4") && !response.Key.StartsWith("5")))
                continue;

            try
            {
                // Si la respuesta tiene content
                if (response.Value?.Content != null && response.Value.Content.Any())
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
                else if (response.Value != null)
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
            catch
            {
                // Si hay algún error al procesar esta respuesta, continuar con la siguiente
                continue;
            }
        }
    }
}