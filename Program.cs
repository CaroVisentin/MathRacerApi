using MathRacerAPI.Infrastructure.Configuration;
using MathRacerAPI.Presentation.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
                .AddApplicationPart(typeof(MathRacerAPI.Presentation.Controllers.HealthController).Assembly);

// Add custom services
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthCheckServices();

var app = builder.Build();

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

app.Run();
