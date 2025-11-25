using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MathRacerAPI.Domain.Repositories;
using MathRacerAPI.Domain.Models;

namespace MathRacerAPI.Infrastructure.Services;

/// <summary>
/// Servicio de fondo que limpia partidas abandonadas cada 5 minutos
/// </summary>
public class GameCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GameCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _gameTimeout = TimeSpan.FromMinutes(10);

    public GameCleanupService(
        IServiceProvider serviceProvider,
        ILogger<GameCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🧹 Servicio de limpieza de partidas iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAbandonedGamesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en limpieza automática de partidas");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupAbandonedGamesAsync()
    {
        // Crear un scope manual para obtener servicios scoped
        using var scope = _serviceProvider.CreateScope();
        var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();

        var games = await gameRepository.GetAllAsync();
        var now = DateTime.UtcNow;
        var abandonedGames = games
            .Where(g => 
                g.Status == GameStatus.WaitingForPlayers &&
                (now - g.CreatedAt) > _gameTimeout)
            .ToList();

        foreach (var game in abandonedGames)
        {
            _logger.LogInformation(
                $"🗑️ Limpiando partida abandonada {game.Id} " +
                $"(creada hace {(now - game.CreatedAt).TotalMinutes:F1} minutos)");

            await gameRepository.DeleteAsync(game.Id);
        }

        if (abandonedGames.Any())
        {
            _logger.LogInformation($"✅ Limpiadas {abandonedGames.Count} partidas abandonadas");
        }
    }
}