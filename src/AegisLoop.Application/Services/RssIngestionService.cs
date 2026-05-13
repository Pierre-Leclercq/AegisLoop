using System.Text.Json;
using AegisLoop.Application.Dtos;
using AegisLoop.Application.Interfaces;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using AegisLoop.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AegisLoop.Application.Services;

public sealed class IngestionService : IIngestionService
{
    private const string SystemActor = "system";
    private readonly IAegisLoopStore _store;
    private readonly INormalizationService _normalizationService;
    private readonly IReadOnlyDictionary<string, ISourceConnector> _sourceConnectors;
    private readonly IConfiguration _configuration;

    public IngestionService(
        IAegisLoopStore store,
        INormalizationService normalizationService,
        IEnumerable<ISourceConnector> sourceConnectors,
        IConfiguration configuration)
    {
        _store = store;
        _normalizationService = normalizationService;
        _sourceConnectors = sourceConnectors.ToDictionary(c => c.ConnectorType, StringComparer.OrdinalIgnoreCase);
        _configuration = configuration;
    }

    public async Task<IngestionResponse> RunAsync(IngestionRequest request, CancellationToken cancellationToken = default)
    {
        var connector = await ResolveConnectorAsync(request, cancellationToken);
        if (!_sourceConnectors.TryGetValue(connector.ConnectorType.ToString(), out var sourceConnector))
        {
            throw new InvalidOperationException($"Connecteur non disponible: {connector.ConnectorType}.");
        }

        var job = await _store.CreateIngestionJobAsync(connector.Id, cancellationToken);
        await AuditAsync("IngestionStarted", connector.Id, new { job.Id, connector.Name }, cancellationToken);

        var errors = new List<string>();
        var created = 0;
        var skipped = 0;
        var normalized = 0;

        try
        {
            var config = BuildConfig(connector.Config, request);
            var validation = await sourceConnector.ValidateConfigAsync(config);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage ?? $"Configuration {connector.ConnectorType} invalide.");
            }

            var result = await sourceConnector.CollectAsync(config, connector.LastRunAt);
            errors.AddRange(result.Errors);

            if (!result.Success)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.DefaultIfEmpty($"Collecte {connector.ConnectorType} échouée.")));
            }

            foreach (var rawItem in result.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                rawItem.ConnectorId = connector.Id;

                if (await _store.SourceHashExistsAsync(rawItem.SourceHash, cancellationToken))
                {
                    skipped++;
                    continue;
                }

                await _store.AddRawItemAsync(rawItem, cancellationToken);
                created++;

                var normalization = await _normalizationService.NormalizeAsync(rawItem);
                if (!normalization.Success || normalization.Observation is null)
                {
                    errors.Add(normalization.ErrorMessage ?? $"Normalisation impossible pour RawItem {rawItem.Id}.");
                    continue;
                }

                await _store.AddObservationAsync(normalization.Observation, cancellationToken);
                normalized++;
            }

            var finalStatus = errors.Count == 0 ? JobStatus.Completed : JobStatus.Completed;
            await _store.CompleteIngestionJobAsync(job.Id, finalStatus, result.ItemsCollected, normalized, errors.Count == 0 ? null : string.Join(" | ", errors), cancellationToken);
            await AuditAsync("IngestionCompleted", connector.Id, new { job.Id, result.ItemsCollected, created, skipped, normalized, errors }, cancellationToken);

            return new IngestionResponse(job.Id, connector.Id, connector.ConnectorType.ToString(), finalStatus.ToString(), result.ItemsCollected, created, skipped, normalized, errors);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errors.Add(ex.Message);
            await _store.CompleteIngestionJobAsync(job.Id, JobStatus.Failed, created + skipped, normalized, ex.Message, cancellationToken);
            await AuditAsync("IngestionError", connector.Id, new { job.Id, error = ex.Message }, cancellationToken);
            return new IngestionResponse(job.Id, connector.Id, connector.ConnectorType.ToString(), JobStatus.Failed.ToString(), created + skipped, created, skipped, normalized, errors);
        }
    }

    private async Task<SourceConnector> ResolveConnectorAsync(IngestionRequest request, CancellationToken cancellationToken)
    {
        if (request.ConnectorId is Guid connectorId)
        {
            var connector = await _store.GetConnectorAsync(connectorId, cancellationToken);
            if (connector is null)
            {
                throw new InvalidOperationException($"Connecteur introuvable: {connectorId}.");
            }

            return connector;
        }

        var connectorType = request.ConnectorType ?? "Rss";
        if (string.Equals(connectorType, "Gdelt", StringComparison.OrdinalIgnoreCase))
        {
            var query = request.Query
                ?? _configuration["DemoGdelt:Query"]
                ?? "Sudan conflict";

            return await _store.EnsureDemoGdeltConnectorAsync(query, cancellationToken);
        }

        if (!string.Equals(connectorType, "Rss", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Type de connecteur non supporté en V1: {connectorType}.");
        }

        var feedUrl = request.FeedUrl
            ?? _configuration["DemoRss:FeedUrl"]
            ?? "https://www.nasa.gov/news-release/feed/";

        return await _store.EnsureDemoRssConnectorAsync(feedUrl, cancellationToken);
    }

    private static string BuildConfig(string connectorConfig, IngestionRequest request)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(connectorConfig) ? "{}" : connectorConfig);
        var values = new Dictionary<string, object?>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            values[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.Number when property.Value.TryGetInt32(out var i) => i,
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => property.Value.GetRawText()
            };
        }

        if (!string.IsNullOrWhiteSpace(request.FeedUrl))
        {
            values["feedUrl"] = request.FeedUrl;
        }

        if (request.MaxItems is > 0)
        {
            values["maxItemsPerPoll"] = request.MaxItems.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            values["query"] = request.Query;
        }

        values.TryAdd("maxItemsPerPoll", 25);
        values.TryAdd("timeoutSeconds", 10);

        return JsonSerializer.Serialize(values);
    }

    private Task AuditAsync(string action, Guid connectorId, object details, CancellationToken cancellationToken)
    {
        return _store.AddAuditEntryAsync(new AuditEntry
        {
            Category = AuditCategory.Ingestion,
            Action = action,
            Actor = SystemActor,
            TargetType = nameof(SourceConnector),
            TargetId = connectorId,
            Details = JsonSerializer.Serialize(details)
        }, cancellationToken);
    }
}
