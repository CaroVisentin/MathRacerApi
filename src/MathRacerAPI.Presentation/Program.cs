using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Presentation.Configuration;
using MathRacerAPI.Presentation.Hubs;
using MathRacerAPI.Presentation.Middleware;
using MathRacerAPI.Infrastructure.Services;
using MercadoPago.Config;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    Env.Load(".env.development");  // solo en local
}

// Configurar URLs
//builder.WebHost.UseUrls("http://localhost:5153");
builder.WebHost.UseUrls("http://10.68.212.238:5153");

// Leer orígenes permitidos desde la configuración
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

// Add services to the container
builder.Services.AddControllers()
                .AddApplicationPart(typeof(MathRacerAPI.Presentation.Controllers.HealthController).Assembly);

// Add custom services
builder.Services.AddApplicationServices(
    builder.Configuration,
    builder.Environment
);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthCheckServices();

// Registrar servicio de Firebase
builder.Services.AddSingleton<FirebaseService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // En desarrollo, permitir orígenes comunes de Live Server y otros servidores de desarrollo
            policy.WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "http://10.68.212.238:5500",
                "http://127.0.0.1:5501",
                "http://10.68.212.238:5501",
                "http://localhost:5501",
                "http://127.0.0.1:5502",
                "http://10.68.212.238:5502",
                "http://localhost:5502",
                "http://127.0.0.1:3000",
                "http://localhost:3000",
                "http://10.68.212.238:5173",
                "http://localhost:5173",
                "https://n3h9h888-5173.brs.devtunnels.ms",
                "https://n3h9h888-5173.brs.devtunnels.ms/",
                "https://n3h9h888-5173.brs.devtunnels.ms/",
                "http://10.5.101.207:5153",
                "http://10.68.212.238:5153"




            )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // En producción, usar orígenes específicos
            policy.WithOrigins(allowedOrigins ?? new[] { 
                "https://mathi-racer-web.vercel.app",
                "http://localhost:3000", 
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "https://n3h9h888-5173.brs.devtunnels.ms/",
                "https://n3h9h888-5173.brs.devtunnels.ms"
            })
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseMiddleware<FirebaseAuthMiddleware>();

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline
app.UseSwaggerDocumentation();

// Deshabilitar redirección HTTPS en desarrollo para evitar problemas con SignalR
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// Map health checks
app.MapHealthChecks("/health");

// Custom endpoints
app.UseCustomEndpoints();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<GameHub>("/gameHub");


var mpToken = builder.Configuration.GetSection("Payment:AccessToken").Get<string>() ;

MercadoPagoConfig.AccessToken = mpToken;

app.Run();
