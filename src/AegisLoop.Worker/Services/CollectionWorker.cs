using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AegisLoop.Worker.Services;

/// <summary>
/// Worker d'ingestion V1 — orchestration RSS + GDELT.
/// Phase 0 bis : boucle de heartbeat minimale, vocabulaire V1.
/// Phase 1 : pipeline Collecte → Normalisation → Observation → Corrélation → Scoring.
/// </summary>
public class IngestionWorker : BackgroundService
{
    private readonly ILogger<IngestionWorker> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public IngestionWorker(ILogger<IngestionWorker> logger, IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AegisLoop Worker V1 démarré — IngestionWorker actif.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("IngestionWorker heartbeat — en attente d'implémentation Phase 1 (ingestion RSS + GDELT)");
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("AegisLoop Worker arrêté.");
    }
}