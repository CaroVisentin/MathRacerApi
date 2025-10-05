using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Presentation.Configuration;
using MathRacerAPI.Presentation.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configurar URLs
builder.WebHost.UseUrls("http://localhost:5153");

// Leer orígenes permitidos desde la configuración
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
// Add services to the container
builder.Services.AddControllers()
                .AddApplicationPart(typeof(MathRacerAPI.Presentation.Controllers.HealthController).Assembly);

// Add custom services
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthCheckServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // En desarrollo, permitir cualquier origen
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // En producción, usar orígenes específicos
            policy.WithOrigins(allowedOrigins ?? new[] { 
                "http://localhost:3000", 
                "http://127.0.0.1:5500",
                "http://localhost:5500"
            })
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();
app.UseAuthorization();

// Map health checks
app.MapHealthChecks("/health");

// Custom endpoints
app.UseCustomEndpoints();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<GameHub>("/gameHub");

app.Run();
