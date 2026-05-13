using AegisLoop.Application;
using AegisLoop.Application.Dtos;
using AegisLoop.Application.Interfaces;
using AegisLoop.Connectors;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using AegisLoop.Domain.Interfaces;
using AegisLoop.Infrastructure;
using AegisLoop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Infrastructure (SQLite)
var connectionString = builder.Environment.IsEnvironment("Testing")
    ? $"Data Source={Path.Combine(Path.GetTempPath(), $"aegisloop-api-test-{Guid.NewGuid():N}.db")}"
    : builder.Configuration.GetConnectionString("AegisLoopDb")
    ?? "Data Source=aegisloop.db";
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddHttpClient<ISourceConnector, RssConnector>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<ISourceConnector, GdeltConnector>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Logging structuré (Serilog en Phase 1)
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
    dbContext.Database.EnsureCreated();
    EnsurePhase3Schema(dbContext);
    EnsureDefaultConnectors(dbContext, builder.Configuration);
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();

app.MapControllers();
app.MapHealthChecks("/health");

// === API V1 — Phase 1 verticale RSS → RawItem → Observation ===

// Events
app.MapGet("/api/events", async (IAegisLoopStore store, int? take, CancellationToken cancellationToken) =>
{
    var events = await store.ListEventCasesAsync(take ?? 100, cancellationToken);
    return Results.Ok(new { success = true, data = events, meta = new { totalCount = events.Count } });
})
    .WithTags("Events");
app.MapGet("/api/events/{id:guid}", async (Guid id, IAegisLoopStore store, CancellationToken cancellationToken) =>
{
    var eventCase = await store.GetEventCaseAsync(id, cancellationToken);
    return eventCase is null
        ? Results.NotFound(new { success = false, error = "EventCase introuvable." })
        : Results.Ok(new { success = true, data = eventCase });
})
    .WithTags("Events");
app.MapPost("/api/events/rebuild", async (IEventCaseService eventCaseService, CancellationToken cancellationToken) =>
{
    var result = await eventCaseService.RebuildAsync(cancellationToken);
    return Results.Ok(new { success = true, data = result });
})
    .WithTags("Events");
app.MapPost("/api/events", () => Results.Ok(new { success = true }))
    .WithTags("Events");
app.MapPatch("/api/events/{id:guid}", (Guid id) => Results.Ok(new { success = true }))
    .WithTags("Events");

// Carte + Timeline V1 — payload sobre, sans objets domaine complets.
app.MapGet("/api/map-timeline", async (AegisLoopDbContext dbContext, string? source, double? minScore, string? scenario, CancellationToken cancellationToken) =>
{
    var payload = await BuildMapTimelineAsync(dbContext, source, minScore, scenario, cancellationToken);
    return Results.Ok(new { success = true, data = payload, meta = new { totalCount = payload.TotalCount } });
})
    .WithTags("MapTimeline");

// Observations
app.MapGet("/api/observations", async (IAegisLoopStore store, int? take, CancellationToken cancellationToken) =>
{
    var observations = await store.ListObservationsAsync(take ?? 100, cancellationToken);
    return Results.Ok(new { success = true, data = observations, meta = new { totalCount = observations.Count } });
})
    .WithTags("Observations");
app.MapGet("/api/observations/{id:guid}", async (Guid id, IAegisLoopStore store, CancellationToken cancellationToken) =>
{
    var observation = await store.GetObservationAsync(id, cancellationToken);
    return observation is null
        ? Results.NotFound(new { success = false, error = "Observation introuvable." })
        : Results.Ok(new { success = true, data = observation });
})
    .WithTags("Observations");
app.MapGet("/api/observations/{id:guid}/provenance", async (Guid id, AegisLoopDbContext dbContext, CancellationToken cancellationToken) =>
{
    var provenance = await BuildObservationProvenanceAsync(id, dbContext, cancellationToken);
    return provenance is null
        ? Results.NotFound(new { success = false, error = "Provenance observation introuvable." })
        : Results.Ok(new { success = true, data = provenance });
})
    .WithTags("Observations");

// Feedback analyste
app.MapPost("/api/feedback", async (SubmitFeedbackRequest request, AegisLoopDbContext dbContext, IScoringService scoringService, CancellationToken cancellationToken) =>
{
    if (!TryParseTargetType(request.TargetType, out var targetType))
    {
        return Results.BadRequest(new { success = false, error = "TargetType doit être Observation ou EventCase." });
    }

    if (!Enum.TryParse<FeedbackAction>(request.Action, ignoreCase: true, out var action))
    {
        return Results.BadRequest(new { success = false, error = "Action feedback non supportée. Valeurs V1: Confirm, Invalidate, Correct, Note." });
    }

    var targetExists = targetType == ScoreTargetType.Observation
        ? await dbContext.Observations.AnyAsync(o => o.Id == request.TargetId, cancellationToken)
        : await dbContext.EventCases.AnyAsync(e => e.Id == request.TargetId, cancellationToken);
    if (!targetExists)
    {
        return Results.NotFound(new { success = false, error = "Cible feedback introuvable." });
    }

    var feedback = new AnalystFeedback
    {
        TargetId = request.TargetId,
        TargetType = targetType.ToString(),
        Action = action,
        Details = request.Details,
        CreatedAt = DateTime.UtcNow,
        IsCancelable = true
    };

    dbContext.AnalystFeedbacks.Add(feedback);
    dbContext.AuditEntries.Add(new AuditEntry
    {
        Category = AuditCategory.Analyst,
        Action = "AnalystFeedbackSubmitted",
        Actor = "analyst-local",
        TargetType = targetType.ToString(),
        TargetId = request.TargetId,
        Details = JsonSerializer.Serialize(new { feedback.Id, action = action.ToString(), request.Details })
    });

    await dbContext.SaveChangesAsync(cancellationToken);
    await scoringService.RecalculateAfterFeedbackAsync(request.TargetId, targetType);

    var score = await scoringService.GetBreakdownAsync(request.TargetId, cancellationToken);
    return Results.Ok(new
    {
        success = true,
        data = new
        {
            feedback = ToFeedbackDto(feedback),
            score
        }
    });
})
    .WithTags("Feedback");

// Connecteurs
app.MapGet("/api/connectors", async (IAegisLoopStore store, CancellationToken cancellationToken) =>
{
    var connectors = await store.ListConnectorsAsync(cancellationToken);
    return Results.Ok(new { success = true, data = connectors, meta = new { totalCount = connectors.Count } });
})
    .WithTags("Connectors");
app.MapGet("/api/connectors/{id:guid}", async (Guid id, IAegisLoopStore store, CancellationToken cancellationToken) =>
{
    var connector = await store.GetConnectorAsync(id, cancellationToken);
    return connector is null
        ? Results.NotFound(new { success = false, error = "Connecteur introuvable." })
        : Results.Ok(new { success = true, data = connector });
})
    .WithTags("Connectors");
app.MapPost("/api/connectors", () => Results.Ok(new { success = true }))
    .WithTags("Connectors");

// Ingestion
app.MapPost("/api/ingestion/run", async (IngestionRequest? request, IIngestionService ingestionService, CancellationToken cancellationToken) =>
{
    var result = await ingestionService.RunAsync(request ?? new IngestionRequest("Rss", null, null, null, null), cancellationToken);
    return Results.Ok(new { success = result.Status != "Failed", data = result, error = result.Status == "Failed" ? string.Join("; ", result.Errors) : null });
})
    .WithTags("Ingestion");
app.MapPost("/api/ingestion/rss/run", async (IngestionRequest? request, IIngestionService ingestionService, CancellationToken cancellationToken) =>
{
    var normalizedRequest = request is null
        ? new IngestionRequest("Rss", null, null, null, null)
        : request with { ConnectorType = "Rss" };
    var result = await ingestionService.RunAsync(normalizedRequest, cancellationToken);
    return Results.Ok(new { success = result.Status != "Failed", data = result, error = result.Status == "Failed" ? string.Join("; ", result.Errors) : null });
})
    .WithTags("Ingestion");
app.MapPost("/api/ingestion/gdelt/run", async (IngestionRequest? request, IIngestionService ingestionService, CancellationToken cancellationToken) =>
{
    var normalizedRequest = request is null
        ? new IngestionRequest("Gdelt", null, null, null, null)
        : request with { ConnectorType = "Gdelt" };
    var result = await ingestionService.RunAsync(normalizedRequest, cancellationToken);
    return Results.Ok(new { success = result.Status != "Failed", data = result, error = result.Status == "Failed" ? string.Join("; ", result.Errors) : null });
})
    .WithTags("Ingestion");
app.MapGet("/api/ingestion/jobs", async (IAegisLoopStore store, CancellationToken cancellationToken) =>
{
    var jobs = await store.ListIngestionJobsAsync(cancellationToken);
    return Results.Ok(new { success = true, data = jobs, meta = new { totalCount = jobs.Count } });
})
    .WithTags("Ingestion");
app.MapGet("/api/ingestion/jobs/{id:guid}", async (Guid id, IAegisLoopStore store, CancellationToken cancellationToken) =>
{
    var job = await store.GetIngestionJobAsync(id, cancellationToken);
    return job is null
        ? Results.NotFound(new { success = false, error = "Job d'ingestion introuvable." })
        : Results.Ok(new { success = true, data = job });
})
    .WithTags("Ingestion");

// Audit
app.MapGet("/api/audit", async (AegisLoopDbContext dbContext, int? take, CancellationToken cancellationToken) =>
{
    var auditEntries = await dbContext.AuditEntries
        .AsNoTracking()
        .OrderByDescending(a => a.Timestamp)
        .Take(Math.Clamp(take ?? 100, 1, 500))
        .ToListAsync(cancellationToken);
    var entries = auditEntries
        .Select(a => new AuditEntryDto(
            a.Id,
            a.Timestamp,
            a.Category.ToString(),
            a.Action,
            a.TargetType,
            a.TargetId,
            a.Details ?? a.Action,
            IsAuditError(a.Action, a.Details) ? "Error" : "Info",
            a.Actor))
        .ToList();
    return Results.Ok(new { success = true, data = entries, meta = new { totalCount = entries.Count } });
})
    .WithTags("Audit");

// Scoring
app.MapGet("/api/scoring/{id:guid}/breakdown", async (Guid id, IScoringService scoringService, CancellationToken cancellationToken) =>
{
    var breakdown = await scoringService.GetBreakdownAsync(id, cancellationToken);
    return breakdown is null
        ? Results.NotFound(new { success = false, error = "Score introuvable pour cette cible." })
        : Results.Ok(new { success = true, data = breakdown });
})
    .WithTags("Scoring");

app.MapGet("/api/events/{id:guid}/provenance", async (Guid id, AegisLoopDbContext dbContext, CancellationToken cancellationToken) =>
{
    var eventCase = await dbContext.EventCases.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    if (eventCase is null)
    {
        return Results.NotFound(new { success = false, error = "EventCase introuvable." });
    }

    var observationIds = await dbContext.Observations.AsNoTracking()
        .Where(o => o.EventCaseId == id)
        .OrderByDescending(o => o.ObservedAt)
        .Select(o => o.Id)
        .ToListAsync(cancellationToken);

    var observations = new List<ObservationProvenanceDto>();
    foreach (var observationId in observationIds)
    {
        var provenance = await BuildObservationProvenanceAsync(observationId, dbContext, cancellationToken);
        if (provenance is not null)
        {
            observations.Add(provenance);
        }
    }

    var eventFeedbackEntities = await dbContext.AnalystFeedbacks.AsNoTracking()
        .Where(f => f.TargetId == id && f.TargetType == nameof(EventCase))
        .OrderByDescending(f => f.CreatedAt)
        .ToListAsync(cancellationToken);
    var eventFeedbacks = eventFeedbackEntities.Select(ToFeedbackDto).ToList();

    var eventScore = await LatestScoreBreakdownAsync(dbContext, id, cancellationToken);
    var sources = observations
        .Select(o => o.SourceConnector)
        .Where(s => s is not null)
        .Select(s => s!)
        .GroupBy(s => s.Id)
        .Select(g => g.First())
        .OrderBy(s => s.Name)
        .ToList();
    var rawItems = observations
        .Select(o => o.RawItem)
        .Where(r => r is not null)
        .Select(r => r!)
        .GroupBy(r => r.Id)
        .Select(g => g.First())
        .OrderByDescending(r => r.CollectedAt)
        .ToList();

    return Results.Ok(new
    {
        success = true,
        data = new EventCaseProvenanceDto(
            eventCase.Id,
            eventCase.Title,
            observations,
            sources,
            rawItems,
            rawItems.Select(r => r.SourceHash).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(h => h).ToList(),
            eventFeedbacks,
            eventScore)
    });
})
    .WithTags("Events");

// Dashboard
app.MapGet("/api/dashboard", async (AegisLoopDbContext dbContext, IAegisLoopStore store, CancellationToken cancellationToken) =>
{
    var lastJob = await dbContext.IngestionJobs
        .AsNoTracking()
        .OrderByDescending(j => j.StartedAt)
        .Select(j => new { j.Id, j.ConnectorId, j.StartedAt, j.CompletedAt, Status = j.Status.ToString(), j.ErrorMessage })
        .FirstOrDefaultAsync(cancellationToken);

    var recentErrors = await dbContext.IngestionJobs
        .AsNoTracking()
        .Where(j => j.ErrorMessage != null)
        .OrderByDescending(j => j.StartedAt)
        .Take(5)
        .Select(j => new { j.Id, j.StartedAt, j.ErrorMessage })
        .ToListAsync(cancellationToken);

    var recentEvents = await store.ListEventCasesAsync(5, cancellationToken);
    var highScoreEvents = recentEvents
        .OrderByDescending(e => e.Score)
        .Take(5)
        .ToList();
    var categoryDistribution = await dbContext.EventCases
        .AsNoTracking()
        .GroupBy(e => e.Category)
        .Select(g => new { category = g.Key.ToString(), count = g.Count() })
        .OrderByDescending(g => g.count)
        .ToListAsync(cancellationToken);
    var sourceDistribution = await (from observation in dbContext.Observations.AsNoTracking()
                                    join connector in dbContext.SourceConnectors.AsNoTracking()
                                        on observation.SourceConnectorId equals connector.Id into connectors
                                    from connector in connectors.DefaultIfEmpty()
                                    where observation.EventCaseId != null
                                    group observation by connector == null ? "Unknown" : connector.ConnectorType.ToString() into grouped
                                    select new { source = grouped.Key, count = grouped.Count() })
        .OrderByDescending(g => g.count)
        .ToListAsync(cancellationToken);

    return Results.Ok(new
    {
        success = true,
        data = new
        {
            connectors = await dbContext.SourceConnectors.CountAsync(cancellationToken),
            rawItems = await dbContext.RawItems.CountAsync(cancellationToken),
            observations = await dbContext.Observations.CountAsync(cancellationToken),
            jobs = await dbContext.IngestionJobs.CountAsync(cancellationToken),
            events = await dbContext.EventCases.CountAsync(cancellationToken),
            contradictions = await dbContext.Contradictions.CountAsync(cancellationToken),
            lastIngestion = lastJob,
            recentErrors,
            recentEvents,
            highScoreEvents,
            categoryDistribution,
            sourceDistribution
        }
    });
})
    .WithTags("Dashboard");

// Export
app.MapGet("/api/export/{id:guid}", async (Guid id, string? format, AegisLoopDbContext dbContext, CancellationToken cancellationToken) =>
{
    var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "json" : format.Trim().ToLowerInvariant();
    if (normalizedFormat is not ("json" or "markdown" or "md"))
    {
        return Results.BadRequest(new { success = false, error = "Format export non supporté. Valeurs V1: json, markdown." });
    }

    var eventCaseExists = await dbContext.EventCases.AnyAsync(e => e.Id == id, cancellationToken);
    if (!eventCaseExists)
    {
        return Results.NotFound(new { success = false, error = "EventCase introuvable." });
    }

    dbContext.AuditEntries.Add(new AuditEntry
    {
        Category = AuditCategory.Configuration,
        Action = "EventCaseExported",
        Actor = "analyst-local",
        TargetType = nameof(EventCase),
        TargetId = id,
        Details = JsonSerializer.Serialize(new { format = normalizedFormat, exportedAt = DateTime.UtcNow })
    });
    await dbContext.SaveChangesAsync(cancellationToken);

    var export = await BuildEventCaseExportAsync(id, dbContext, cancellationToken);
    if (export is null)
    {
        return Results.NotFound(new { success = false, error = "EventCase introuvable." });
    }

    if (normalizedFormat is "markdown" or "md")
    {
        return Results.Text(BuildEventCaseMarkdown(export), "text/markdown; charset=utf-8");
    }

    return Results.Json(new { success = true, data = export }, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });
})
    .WithTags("Export");

// Demo
app.MapGet("/api/demo/status", async (AegisLoopDbContext dbContext, CancellationToken cancellationToken) =>
{
    var status = await BuildDemoStatusAsync(dbContext, cancellationToken);
    return Results.Ok(new { success = true, data = status });
})
    .WithTags("Demo");
app.MapPost("/api/demo/load", async (AegisLoopDbContext dbContext, IEventCaseService eventCaseService, CancellationToken cancellationToken) =>
{
    var result = await LoadDemoSeedAsync(dbContext, eventCaseService, cancellationToken);
    return Results.Ok(new { success = true, data = result });
})
    .WithTags("Demo");
app.MapPost("/api/demo/reset", async (AegisLoopDbContext dbContext, CancellationToken cancellationToken) =>
{
    var result = await ResetDemoDataAsync(dbContext, addAudit: true, cancellationToken);
    return Results.Ok(new { success = true, data = result });
})
    .WithTags("Demo");
app.MapPost("/api/demo/rebuild", async (AegisLoopDbContext dbContext, IEventCaseService eventCaseService, CancellationToken cancellationToken) =>
{
    var rebuild = await eventCaseService.RebuildAsync(cancellationToken);
    dbContext.AuditEntries.Add(new AuditEntry
    {
        Category = AuditCategory.Configuration,
        Action = "DemoEventCasesRebuilt",
        Actor = "system-demo",
        TargetType = nameof(EventCase),
        Details = JsonSerializer.Serialize(new { rebuild.ObservationsProcessed, rebuild.EventCasesCreated, rebuild.LinksCreated })
    });
    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(new { success = true, data = rebuild });
})
    .WithTags("Demo");
app.MapPost("/api/demo/recalculate", async (AegisLoopDbContext dbContext, IScoringService scoringService, CancellationToken cancellationToken) =>
{
    var observationIds = await dbContext.Observations.AsNoTracking().OrderBy(o => o.ObservedAt).Select(o => o.Id).ToListAsync(cancellationToken);
    var eventIds = await dbContext.EventCases.AsNoTracking().OrderBy(e => e.StartedAt).Select(e => e.Id).ToListAsync(cancellationToken);
    dbContext.ConfidenceScores.RemoveRange(dbContext.ConfidenceScores);
    await dbContext.SaveChangesAsync(cancellationToken);

    foreach (var observationId in observationIds)
    {
        await scoringService.ScoreObservationAsync(observationId, cancellationToken);
    }

    foreach (var eventId in eventIds)
    {
        await scoringService.ScoreEventAsync(eventId, cancellationToken);
    }

    dbContext.AuditEntries.Add(new AuditEntry
    {
        Category = AuditCategory.Configuration,
        Action = "DemoScoresRecalculated",
        Actor = "system-demo",
        TargetType = nameof(ConfidenceScore),
        Details = JsonSerializer.Serialize(new { observations = observationIds.Count, eventCases = eventIds.Count })
    });
    await dbContext.SaveChangesAsync(cancellationToken);
    var status = await BuildDemoStatusAsync(dbContext, cancellationToken);
    return Results.Ok(new { success = true, data = status });
})
    .WithTags("Demo");

// SSE notifications
app.MapGet("/api/events/stream", (HttpContext context) =>
{
    // Phase 1 : SSE stream pour notifications temps réel
    return Results.Ok();
}).WithTags("SSE");

app.Run();

static void EnsureDefaultConnectors(AegisLoopDbContext dbContext, IConfiguration configuration)
{
    var rssUrl = configuration["DemoRss:FeedUrl"] ?? "https://www.nasa.gov/news-release/feed/";
    var gdeltQuery = configuration["DemoGdelt:Query"] ?? "Sudan conflict";

    if (!dbContext.SourceConnectors.Any(c => c.ConnectorType == ConnectorType.Rss && c.Name == "RSS Démo"))
    {
        dbContext.SourceConnectors.Add(new SourceConnector
        {
            ConnectorType = ConnectorType.Rss,
            Name = "RSS Démo",
            Config = System.Text.Json.JsonSerializer.Serialize(new { feedUrl = rssUrl, pollingIntervalMinutes = 15, maxItemsPerPoll = 25, timeoutSeconds = 10 }),
            Status = ConnectorStatus.Active
        });
    }

    if (!dbContext.SourceConnectors.Any(c => c.ConnectorType == ConnectorType.Gdelt && c.Name == "GDELT Démo"))
    {
        dbContext.SourceConnectors.Add(new SourceConnector
        {
            ConnectorType = ConnectorType.Gdelt,
            Name = "GDELT Démo",
            Config = System.Text.Json.JsonSerializer.Serialize(new { query = gdeltQuery, maxItemsPerPoll = 25, timeoutSeconds = 10 }),
            Status = ConnectorStatus.Active
        });
    }

    dbContext.SaveChanges();
}

static void EnsurePhase3Schema(AegisLoopDbContext dbContext)
{
    AddColumnIfMissing(dbContext, "Observations", "EventCaseId", "TEXT NULL");
    AddColumnIfMissing(dbContext, "EventCases", "CreatedAt", "TEXT NOT NULL DEFAULT '1970-01-01 00:00:00'");
    AddColumnIfMissing(dbContext, "EventCases", "UpdatedAt", "TEXT NOT NULL DEFAULT '1970-01-01 00:00:00'");
}

static void AddColumnIfMissing(AegisLoopDbContext dbContext, string tableName, string columnName, string columnDefinition)
{
    if (!IsKnownPhase3SchemaEdit(tableName, columnName, columnDefinition))
    {
        throw new InvalidOperationException("Modification de schéma Phase 3 non autorisée.");
    }

#pragma warning disable EF1002 // SQL borné à une allow-list locale, aucun input utilisateur.
    var columns = dbContext.Database.SqlQueryRaw<string>($"SELECT name AS Value FROM pragma_table_info('{tableName}')")
        .ToList();
    if (!columns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
    {
        dbContext.Database.ExecuteSqlRaw($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}");
    }
#pragma warning restore EF1002
}

static bool IsKnownPhase3SchemaEdit(string tableName, string columnName, string columnDefinition)
{
    return (tableName, columnName, columnDefinition) is
        ("Observations", "EventCaseId", "TEXT NULL") or
        ("EventCases", "CreatedAt", "TEXT NOT NULL DEFAULT '1970-01-01 00:00:00'") or
        ("EventCases", "UpdatedAt", "TEXT NOT NULL DEFAULT '1970-01-01 00:00:00'");
}

static async Task<ObservationProvenanceDto?> BuildObservationProvenanceAsync(Guid id, AegisLoopDbContext dbContext, CancellationToken cancellationToken)
{
    var observation = await dbContext.Observations.AsNoTracking()
        .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    if (observation is null)
    {
        return null;
    }

    var connector = await dbContext.SourceConnectors.AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == observation.SourceConnectorId, cancellationToken);
    var rawItem = observation.RawItemId is Guid rawItemId
        ? await dbContext.RawItems.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rawItemId, cancellationToken)
        : null;
    var ingestionJob = rawItem is null
        ? null
        : await dbContext.IngestionJobs.AsNoTracking()
            .Where(j => j.ConnectorId == rawItem.ConnectorId && j.StartedAt <= rawItem.CollectedAt)
            .OrderByDescending(j => j.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

    var feedbackEntities = await dbContext.AnalystFeedbacks.AsNoTracking()
        .Where(f => f.TargetId == observation.Id && f.TargetType == nameof(Observation))
        .OrderByDescending(f => f.CreatedAt)
        .ToListAsync(cancellationToken);
    var feedbacks = feedbackEntities.Select(ToFeedbackDto).ToList();
    var score = await LatestScoreBreakdownAsync(dbContext, observation.Id, cancellationToken);

    return new ObservationProvenanceDto(
        observation.Id,
        observation.Title,
        observation.EventCaseId,
        observation.ObservedAt,
        observation.SourceUrl ?? rawItem?.SourceUrl,
        connector is null ? null : new SourceConnectorProvenanceDto(
            connector.Id,
            connector.Name,
            connector.ConnectorType.ToString(),
            connector.Status.ToString(),
            connector.LastRunAt),
        rawItem is null ? null : new RawItemProvenanceDto(
            rawItem.Id,
            rawItem.SourceHash,
            rawItem.ContentType.ToString(),
            rawItem.CollectedAt,
            rawItem.PublishedAt,
            rawItem.SourceUrl,
            ExtractRawMetadata(rawItem.RawContent)),
        ingestionJob is null ? null : new IngestionJobProvenanceDto(
            ingestionJob.Id,
            ingestionJob.StartedAt,
            ingestionJob.CompletedAt,
            ingestionJob.Status.ToString(),
            ingestionJob.ItemsCollected,
            ingestionJob.ItemsNormalized,
            ingestionJob.ErrorMessage),
        score,
        feedbacks,
        ParseMetadataJson(observation.MetadataJson));
}

static async Task<MapTimelineResponseDto> BuildMapTimelineAsync(AegisLoopDbContext dbContext, string? source, double? minScore, string? scenario, CancellationToken cancellationToken)
{
    var events = await dbContext.EventCases.AsNoTracking()
        .OrderBy(e => e.StartedAt)
        .ToListAsync(cancellationToken);
    if (events.Count == 0)
    {
        return new MapTimelineResponseDto(Array.Empty<MapTimelineItemDto>(), 0, 0, null, null, Array.Empty<string>(), Array.Empty<string>());
    }

    var eventIds = events.Select(e => e.Id).ToHashSet();
    var observations = await dbContext.Observations.AsNoTracking()
        .Where(o => o.EventCaseId != null && eventIds.Contains(o.EventCaseId.Value))
        .OrderBy(o => o.ObservedAt)
        .ToListAsync(cancellationToken);
    var connectorIds = observations.Select(o => o.SourceConnectorId).Distinct().ToHashSet();
    var connectors = await dbContext.SourceConnectors.AsNoTracking()
        .Where(c => connectorIds.Contains(c.Id))
        .ToDictionaryAsync(c => c.Id, cancellationToken);
    var locationIds = events.Select(e => e.LocationId)
        .Concat(observations.Select(o => o.GeoLocationId))
        .Where(id => id is not null)
        .Select(id => id!.Value)
        .Distinct()
        .ToHashSet();
    var locations = await dbContext.Locations.AsNoTracking()
        .Where(l => locationIds.Contains(l.Id))
        .ToDictionaryAsync(l => l.Id, cancellationToken);
    var eventScores = await dbContext.ConfidenceScores.AsNoTracking()
        .Where(s => eventIds.Contains(s.TargetId) && s.TargetType == ScoreTargetType.EventCase)
        .ToListAsync(cancellationToken);
    var latestScores = eventScores
        .GroupBy(s => s.TargetId)
        .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.CalculatedAt).First().Value);

    var items = events.Select(eventCase =>
        {
            var eventObservations = observations.Where(o => o.EventCaseId == eventCase.Id).ToList();
            var sourceNames = eventObservations
                .Select(o => connectors.TryGetValue(o.SourceConnectorId, out var connector) ? connector.Name : "Source inconnue")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            var sourceTypes = eventObservations
                .Select(o => connectors.TryGetValue(o.SourceConnectorId, out var connector) ? connector.ConnectorType.ToString() : "Unknown")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            var firstMetadata = eventObservations
                .Select(o => ParseMetadataJson(o.MetadataJson))
                .FirstOrDefault(m => m.Count > 0) ?? new Dictionary<string, string?>();

            var locationId = eventCase.LocationId ?? eventObservations
                .Where(o => o.GeoLocationId is not null)
                .GroupBy(o => o.GeoLocationId!.Value)
                .OrderByDescending(g => g.Count())
                .Select(g => (Guid?)g.Key)
                .FirstOrDefault();
            locations.TryGetValue(locationId ?? Guid.Empty, out var location);

            return new MapTimelineItemDto(
                eventCase.Id,
                eventCase.Title,
                latestScores.TryGetValue(eventCase.Id, out var score) ? score : 0.0,
                eventCase.Status.ToString(),
                eventCase.Category.ToString(),
                eventCase.StartedAt,
                sourceNames,
                sourceTypes,
                eventObservations.Count,
                location?.Latitude,
                location?.Longitude,
                location?.Name ?? (firstMetadata.TryGetValue("locationName", out var locationName) ? locationName : null),
                firstMetadata.TryGetValue("region", out var region) ? region : null,
                firstMetadata.TryGetValue("country", out var country) ? country : null,
                firstMetadata.TryGetValue("scenario", out var scenarioValue) ? scenarioValue : null,
                firstMetadata.TryGetValue("scenarioLabel", out var scenarioLabel) ? scenarioLabel : null);
        })
        .ToList();

    var normalizedSource = string.IsNullOrWhiteSpace(source) || source.Equals("all", StringComparison.OrdinalIgnoreCase) ? null : source.Trim();
    if (normalizedSource is not null)
    {
        items = items.Where(i => i.SourceTypes.Any(s => s.Equals(normalizedSource, StringComparison.OrdinalIgnoreCase))).ToList();
    }

    if (minScore is double threshold)
    {
        items = items.Where(i => i.Score >= Math.Clamp(threshold, 0.0, 1.0)).ToList();
    }

    if (!string.IsNullOrWhiteSpace(scenario) && !scenario.Equals("all", StringComparison.OrdinalIgnoreCase))
    {
        items = items.Where(i => string.Equals(i.Scenario, scenario.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
    }

    var periodStart = items.Count == 0 ? null : (DateTime?)items.Min(i => i.Date);
    var periodEnd = items.Count == 0 ? null : (DateTime?)items.Max(i => i.Date);
    return new MapTimelineResponseDto(
        items.OrderBy(i => i.Date).ToList(),
        items.Count,
        items.Count(i => i.Latitude is null || i.Longitude is null),
        periodStart,
        periodEnd,
        items.Select(i => i.Scenario).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList(),
        items.SelectMany(i => i.SourceTypes).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList());
}

static async Task<ScoreBreakdownDto?> LatestScoreBreakdownAsync(AegisLoopDbContext dbContext, Guid targetId, CancellationToken cancellationToken)
{
    var score = await dbContext.ConfidenceScores.AsNoTracking()
        .Where(s => s.TargetId == targetId)
        .OrderByDescending(s => s.CalculatedAt)
        .FirstOrDefaultAsync(cancellationToken);

    return score is null ? null : EfScoringService.ToBreakdown(score);
}

static bool TryParseTargetType(string value, out ScoreTargetType targetType)
{
    if (Enum.TryParse(value, ignoreCase: true, out targetType))
    {
        return targetType is ScoreTargetType.Observation or ScoreTargetType.EventCase;
    }

    targetType = default;
    return false;
}

static FeedbackDto ToFeedbackDto(AnalystFeedback feedback)
{
    return new FeedbackDto(feedback.Id, feedback.TargetId, feedback.TargetType, feedback.Action.ToString(), feedback.Details, feedback.CreatedAt);
}

static bool IsAuditError(string action, string? details)
{
    return action.Contains("Error", StringComparison.OrdinalIgnoreCase)
        || action.Contains("Failed", StringComparison.OrdinalIgnoreCase)
        || (details?.Contains("error", StringComparison.OrdinalIgnoreCase) ?? false);
}

static IReadOnlyDictionary<string, string?> ParseMetadataJson(string? metadataJson)
{
    if (string.IsNullOrWhiteSpace(metadataJson))
    {
        return new Dictionary<string, string?>();
    }

    try
    {
        return JsonSerializer.Deserialize<Dictionary<string, string?>>(metadataJson) ?? new Dictionary<string, string?>();
    }
    catch (JsonException)
    {
        return new Dictionary<string, string?> { ["raw"] = metadataJson };
    }
}

static IReadOnlyDictionary<string, string?> ExtractRawMetadata(string rawContent)
{
    var metadata = new Dictionary<string, string?>();
    if (string.IsNullOrWhiteSpace(rawContent))
    {
        return metadata;
    }

    try
    {
        using var document = JsonDocument.Parse(rawContent);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Name is "source" or "feedTitle" or "domain" or "sourceCountry" or "query" or "language" or "title" or "link" or "url")
            {
                metadata[property.Name] = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : property.Value.GetRawText();
            }
        }
    }
    catch (JsonException)
    {
        metadata["rawContentParseError"] = "RawContent non JSON ou invalide.";
    }

    return metadata;
}

static async Task<DemoStatusDto> BuildDemoStatusAsync(AegisLoopDbContext dbContext, CancellationToken cancellationToken)
{
    var demoObservations = await dbContext.Observations.AsNoTracking()
        .Where(o => o.MetadataJson != null && o.MetadataJson.Contains("v1-seed-2026-04"))
        .ToListAsync(cancellationToken);
    var demoObservationIds = demoObservations.Select(o => o.Id).ToHashSet();
    var scenarios = demoObservations
        .Select(o => ParseMetadataJson(o.MetadataJson).TryGetValue("scenario", out var scenario) ? scenario : null)
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(s => s)
        .Select(s => s!)
        .ToList();

    var demoEventCount = demoObservations
        .Where(o => o.EventCaseId is not null)
        .Select(o => o.EventCaseId!.Value)
        .Distinct()
        .Count();
    var demoRawItems = await dbContext.RawItems.AsNoTracking()
        .CountAsync(r => r.SourceUrl != null && r.SourceUrl.Contains("demo.aegisloop.local"), cancellationToken);
    var demoConnectors = await dbContext.SourceConnectors.AsNoTracking()
        .CountAsync(c => c.Name.Contains("Démo Local"), cancellationToken);
    var demoFeedbacks = await dbContext.AnalystFeedbacks.AsNoTracking()
        .CountAsync(f => demoObservationIds.Contains(f.TargetId), cancellationToken);
    var demoAudits = await dbContext.AuditEntries.AsNoTracking()
        .CountAsync(a => a.Action.StartsWith("Demo") || a.Action == "EventCaseExported", cancellationToken);

    return new DemoStatusDto(
        DatasetVersion: "v1-seed-2026-04",
        SeedPath: FindDemoSeedPath(),
        Loaded: demoObservations.Count > 0,
        Connectors: demoConnectors,
        RawItems: demoRawItems,
        Observations: demoObservations.Count,
        EventCases: demoEventCount,
        Scores: await dbContext.ConfidenceScores.CountAsync(cancellationToken),
        Feedbacks: demoFeedbacks,
        AuditEntries: demoAudits,
        Scenarios: scenarios);
}

static async Task<object> LoadDemoSeedAsync(AegisLoopDbContext dbContext, IEventCaseService eventCaseService, CancellationToken cancellationToken)
{
    await ResetDemoDataAsync(dbContext, addAudit: false, cancellationToken);
    var seed = await ReadDemoSeedAsync(cancellationToken);
    var now = DateTime.UtcNow;
    var connectorByKey = new Dictionary<string, SourceConnector>(StringComparer.OrdinalIgnoreCase);

    foreach (var connectorSeed in seed.Connectors)
    {
        if (!Enum.TryParse<ConnectorType>(connectorSeed.Type, ignoreCase: true, out var connectorType))
        {
            throw new InvalidOperationException($"Connecteur seed non supporté: {connectorSeed.Type}");
        }

        var connector = new SourceConnector
        {
            Id = StableGuid($"connector:{connectorSeed.Key}"),
            ConnectorType = connectorType,
            Name = connectorSeed.Name,
            Config = connectorSeed.Config.GetRawText(),
            Status = ConnectorStatus.Active,
            LastRunAt = now
        };
        connectorByKey[connectorSeed.Key] = connector;
        dbContext.SourceConnectors.Add(connector);
        dbContext.IngestionJobs.Add(new IngestionJob
        {
            Id = StableGuid($"job:{connectorSeed.Key}"),
            ConnectorId = connector.Id,
            StartedAt = now.AddMinutes(-5),
            CompletedAt = now.AddMinutes(-4),
            Status = JobStatus.Completed,
            ItemsCollected = 0,
            ItemsNormalized = 0
        });
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    var observationByIndex = new Dictionary<int, Observation>();
    var index = 0;
    var baseTime = new DateTime(2026, 04, 28, 08, 00, 00, DateTimeKind.Utc);
    for (var groupIndex = 0; groupIndex < seed.Groups.Count; groupIndex++)
    {
        var group = seed.Groups[groupIndex];
        Guid? locationId = null;
        if (group.Location is not null)
        {
            locationId = StableGuid($"location:{group.Keyword}");
            dbContext.Locations.Add(new Location
            {
                Id = locationId.Value,
                Name = group.Location.Name,
                Latitude = Math.Clamp(group.Location.Latitude, -90.0, 90.0),
                Longitude = Math.Clamp(group.Location.Longitude, -180.0, 180.0),
                SourceType = "Seed"
            });
        }

        for (var itemIndex = 0; itemIndex < group.Count; itemIndex++)
        {
            index++;
            var connectorKey = group.ConnectorKeys[itemIndex % group.ConnectorKeys.Count];
            var connector = connectorByKey[connectorKey];
            var observedAt = baseTime.AddHours(groupIndex * 100).AddMinutes(itemIndex * 7);
            var source = connector.ConnectorType == ConnectorType.Rss ? "rss" : "gdelt";
            var title = $"{group.TitleBase} — update {itemIndex + 1:00}";
            var sourceUrl = $"https://demo.aegisloop.local/{group.Scenario}/{groupIndex + 1}/{itemIndex + 1}";
            var rawContent = JsonSerializer.Serialize(new
            {
                source,
                scenario = group.Scenario,
                group = group.Keyword,
                location = group.Location is null ? null : new { group.Location.Name, group.Location.Region, group.Location.Country, group.Location.Latitude, group.Location.Longitude },
                title,
                url = sourceUrl,
                language = "en"
            });
            var raw = new RawItem
            {
                Id = StableGuid($"raw:{index:000}:{group.Keyword}"),
                ConnectorId = connector.Id,
                SourceHash = StableSha256($"raw:{index:000}:{group.Keyword}"),
                RawContent = rawContent,
                ContentType = RawContentType.Json,
                CollectedAt = observedAt,
                PublishedAt = observedAt.AddMinutes(-12),
                SourceUrl = sourceUrl
            };
            var connectorReliability = seed.Connectors.First(c => c.Key.Equals(connectorKey, StringComparison.OrdinalIgnoreCase)).Reliability;
            var reliability = Math.Round(Math.Clamp(connectorReliability + ((itemIndex % 3) - 1) * 0.04, 0.3, 0.95), 2, MidpointRounding.AwayFromZero);
            var observation = new Observation
            {
                Id = StableGuid($"observation:{index:000}:{group.Keyword}"),
                RawItemId = raw.Id,
                Title = title,
                Content = $"{group.ContentBase}. Demo fixture item {itemIndex + 1:00} from {connector.Name} keeps recurring terms for deterministic EventCase clustering.",
                ClaimText = $"{group.Keyword} confirmed by {source} fixture {itemIndex + 1:00}",
                Type = ObservationType.Article,
                Status = ObservationStatus.New,
                ObservedAt = observedAt,
                SourceConnectorId = connector.Id,
                SourceUrl = sourceUrl,
                SourceReliability = reliability,
                Language = "en",
                GeoLocationId = locationId,
                MetadataJson = JsonSerializer.Serialize(new Dictionary<string, string?>
                {
                    ["demoDataset"] = seed.DatasetVersion,
                    ["scenario"] = group.Scenario,
                    ["scenarioLabel"] = group.ScenarioLabel,
                    ["group"] = group.Keyword,
                    ["categoryHint"] = group.CategoryHint,
                    ["locationName"] = group.Location?.Name,
                    ["region"] = group.Location?.Region,
                    ["country"] = group.Location?.Country,
                    ["latitude"] = group.Location?.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["longitude"] = group.Location?.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["fixtureIndex"] = index.ToString("000")
                })
            };

            dbContext.RawItems.Add(raw);
            dbContext.Observations.Add(observation);
            observationByIndex[index] = observation;
        }
    }

    foreach (var feedbackSeed in seed.Feedbacks)
    {
        if (observationByIndex.TryGetValue(feedbackSeed.ObservationIndex, out var target)
            && Enum.TryParse<FeedbackAction>(feedbackSeed.Action, ignoreCase: true, out var action))
        {
            dbContext.AnalystFeedbacks.Add(new AnalystFeedback
            {
                Id = StableGuid($"feedback:{feedbackSeed.ObservationIndex:000}"),
                TargetId = target.Id,
                TargetType = nameof(Observation),
                Action = action,
                Details = feedbackSeed.Details,
                CreatedAt = target.ObservedAt.AddMinutes(30),
                IsCancelable = false
            });
        }
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    foreach (var job in await dbContext.IngestionJobs.Where(j => connectorByKey.Values.Select(c => c.Id).Contains(j.ConnectorId)).ToListAsync(cancellationToken))
    {
        job.ItemsCollected = await dbContext.RawItems.CountAsync(r => r.ConnectorId == job.ConnectorId, cancellationToken);
        job.ItemsNormalized = await dbContext.Observations.CountAsync(o => o.SourceConnectorId == job.ConnectorId, cancellationToken);
    }

    await dbContext.SaveChangesAsync(cancellationToken);
    var rebuild = await eventCaseService.RebuildAsync(cancellationToken);
    dbContext.AuditEntries.Add(new AuditEntry
    {
        Category = AuditCategory.Configuration,
        Action = "DemoSeedLoaded",
        Actor = "system-demo",
        TargetType = nameof(Observation),
        Details = JsonSerializer.Serialize(new { seed.DatasetVersion, observations = index, connectors = connectorByKey.Count, rebuild.EventCasesCreated })
    });
    await dbContext.SaveChangesAsync(cancellationToken);

    return new { status = await BuildDemoStatusAsync(dbContext, cancellationToken), rebuild };
}

static async Task<object> ResetDemoDataAsync(AegisLoopDbContext dbContext, bool addAudit, CancellationToken cancellationToken)
{
    var counts = new
    {
        observations = await dbContext.Observations.CountAsync(cancellationToken),
        rawItems = await dbContext.RawItems.CountAsync(cancellationToken),
        eventCases = await dbContext.EventCases.CountAsync(cancellationToken),
        scores = await dbContext.ConfidenceScores.CountAsync(cancellationToken),
        feedbacks = await dbContext.AnalystFeedbacks.CountAsync(cancellationToken),
        jobs = await dbContext.IngestionJobs.CountAsync(cancellationToken)
    };

    dbContext.Contradictions.RemoveRange(dbContext.Contradictions);
    dbContext.ConfidenceScores.RemoveRange(dbContext.ConfidenceScores);
    dbContext.AnalystFeedbacks.RemoveRange(dbContext.AnalystFeedbacks);
    dbContext.EventCases.RemoveRange(dbContext.EventCases);
    dbContext.Observations.RemoveRange(dbContext.Observations);
    dbContext.RawItems.RemoveRange(dbContext.RawItems);
    dbContext.IngestionJobs.RemoveRange(dbContext.IngestionJobs);
    dbContext.Locations.RemoveRange(dbContext.Locations.Where(l => l.SourceType == "Seed"));
    dbContext.SourceConnectors.RemoveRange(dbContext.SourceConnectors.Where(c => c.Name.Contains("Démo Local")));

    if (addAudit)
    {
        dbContext.AuditEntries.Add(new AuditEntry
        {
            Category = AuditCategory.Configuration,
            Action = "DemoReset",
            Actor = "system-demo",
            Details = JsonSerializer.Serialize(counts)
        });
    }

    await dbContext.SaveChangesAsync(cancellationToken);
    return new { removed = counts, status = await BuildDemoStatusAsync(dbContext, cancellationToken) };
}

static async Task<DemoSeedFile> ReadDemoSeedAsync(CancellationToken cancellationToken)
{
    var path = FindDemoSeedPath();
    await using var stream = File.OpenRead(path);
    return await JsonSerializer.DeserializeAsync<DemoSeedFile>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web), cancellationToken)
        ?? throw new InvalidOperationException("Dataset seed démo invalide.");
}

static string FindDemoSeedPath()
{
    var candidates = new List<string>
    {
        Path.Combine(Directory.GetCurrentDirectory(), "examples", "demo-data", "v1-seed.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "examples", "demo-data", "v1-seed.json"),
        Path.Combine(AppContext.BaseDirectory, "examples", "demo-data", "v1-seed.json"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "examples", "demo-data", "v1-seed.json"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "examples", "demo-data", "v1-seed.json")
    };

    var probe = new DirectoryInfo(AppContext.BaseDirectory);
    for (var depth = 0; probe is not null && depth < 10; depth++, probe = probe.Parent)
    {
        candidates.Add(Path.Combine(probe.FullName, "examples", "demo-data", "v1-seed.json"));
    }

    foreach (var candidate in candidates)
    {
        var fullPath = Path.GetFullPath(candidate);
        if (File.Exists(fullPath))
        {
            return fullPath;
        }
    }

    throw new FileNotFoundException("Dataset seed démo introuvable.", "examples/demo-data/v1-seed.json");
}

static async Task<EventCaseExportDto?> BuildEventCaseExportAsync(Guid id, AegisLoopDbContext dbContext, CancellationToken cancellationToken)
{
    var eventCase = await dbContext.EventCases.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    if (eventCase is null)
    {
        return null;
    }

    var observationIds = await dbContext.Observations.AsNoTracking()
        .Where(o => o.EventCaseId == id)
        .OrderBy(o => o.ObservedAt)
        .Select(o => o.Id)
        .ToListAsync(cancellationToken);
    var observations = new List<ObservationProvenanceDto>();
    foreach (var observationId in observationIds)
    {
        var provenance = await BuildObservationProvenanceAsync(observationId, dbContext, cancellationToken);
        if (provenance is not null)
        {
            observations.Add(provenance);
        }
    }

    var eventFeedbackEntities = await dbContext.AnalystFeedbacks.AsNoTracking()
        .Where(f => f.TargetId == id && f.TargetType == nameof(EventCase))
        .OrderByDescending(f => f.CreatedAt)
        .ToListAsync(cancellationToken);
    var eventFeedbacks = eventFeedbackEntities.Select(ToFeedbackDto).ToList();
    var sources = observations.Select(o => o.SourceConnector).Where(s => s is not null).Select(s => s!).GroupBy(s => s.Id).Select(g => g.First()).OrderBy(s => s.Name).ToList();
    var rawItems = observations.Select(o => o.RawItem).Where(r => r is not null).Select(r => r!).GroupBy(r => r.Id).Select(g => g.First()).OrderByDescending(r => r.CollectedAt).ToList();
    var score = await LatestScoreBreakdownAsync(dbContext, id, cancellationToken);
    var targetIds = observationIds.Append(id).ToHashSet();
    var auditEntities = await dbContext.AuditEntries.AsNoTracking()
        .Where(a => (a.TargetId != null && targetIds.Contains(a.TargetId.Value)) || a.Action == "EventCaseExported")
        .OrderByDescending(a => a.Timestamp)
        .Take(50)
        .ToListAsync(cancellationToken);
    var audit = auditEntities
        .Select(a => new AuditEntryDto(a.Id, a.Timestamp, a.Category.ToString(), a.Action, a.TargetType, a.TargetId, a.Details ?? a.Action, IsAuditError(a.Action, a.Details) ? "Error" : "Info", a.Actor))
        .ToList();

    return new EventCaseExportDto(
        new ExportMetadataDto("AegisLoop", "MVP Solo V1", "1.0", DateTime.UtcNow, "analyst-local", id),
        new EventCaseExportCaseDto(eventCase.Id, eventCase.Title, eventCase.Summary, eventCase.Category.ToString(), eventCase.Status.ToString(), eventCase.StartedAt, eventCase.EndedAt, eventCase.CorroborationCount, eventCase.CreatedAt, eventCase.UpdatedAt),
        score,
        new EventCaseProvenanceDto(eventCase.Id, eventCase.Title, observations, sources, rawItems, rawItems.Select(r => r.SourceHash).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(h => h).ToList(), eventFeedbacks, score),
        eventFeedbacks,
        audit,
        new[]
        {
            "Export V1 local, non signé cryptographiquement.",
            "Scoring heuristique explicable en 3 composantes ; pas de ML/NLP avancé.",
            "Les données seed sont simulées et non sensibles."
        });
}

static string BuildEventCaseMarkdown(EventCaseExportDto export)
{
    var builder = new StringBuilder();
    builder.AppendLine($"# {export.EventCase.Title}");
    builder.AppendLine();
    builder.AppendLine($"Export AegisLoop {export.Metadata.ExportVersion} — {export.Metadata.ExportedAt:u}");
    builder.AppendLine();
    builder.AppendLine("## Résumé");
    builder.AppendLine(export.EventCase.Summary ?? "Aucun résumé disponible.");
    builder.AppendLine();
    builder.AppendLine("## Score");
    builder.AppendLine(export.Score is null ? "Aucun score disponible." : $"Score global : **{export.Score.Value:P0}** (`{export.Score.AlgorithmVersion}`)");
    if (export.Score is not null)
    {
        foreach (var component in export.Score.Components)
        {
            builder.AppendLine($"- {component.Name} : {component.Value:0.00} × poids {component.Weight:0.00} = {component.Contribution:0.00} — {component.Explanation}");
        }
    }
    builder.AppendLine();
    builder.AppendLine("## Sources");
    foreach (var source in export.Provenance.Sources)
    {
        builder.AppendLine($"- {source.Name} ({source.ConnectorType}) — statut {source.Status}");
    }
    builder.AppendLine();
    builder.AppendLine("## Observations");
    foreach (var observation in export.Provenance.Observations)
    {
        builder.AppendLine($"### {observation.Title}");
        builder.AppendLine($"- Observée : {observation.ObservedAt:u}");
        builder.AppendLine($"- Source : {observation.SourceConnector?.Name ?? "n/a"}");
        builder.AppendLine($"- URL : {observation.SourceUrl ?? "n/a"}");
        builder.AppendLine($"- Hash RawItem : {observation.RawItem?.SourceHash ?? "n/a"}");
        builder.AppendLine($"- Score : {(observation.Score is null ? "n/a" : observation.Score.Value.ToString("P0"))}");
        if (observation.Feedbacks.Count > 0)
        {
            builder.AppendLine("- Feedbacks observation : " + string.Join("; ", observation.Feedbacks.Select(f => $"{f.Action} ({f.CreatedAt:u}) {f.Details}")));
        }
        builder.AppendLine();
    }
    builder.AppendLine("## Provenance");
    builder.AppendLine($"RawItems : {export.Provenance.RawItems.Count} — Hashes : {string.Join(", ", export.Provenance.Hashes)}");
    builder.AppendLine();
    builder.AppendLine("## Feedback analyste");
    if (export.Feedbacks.Count == 0)
    {
        builder.AppendLine("Aucun feedback EventCase. Les feedbacks observation sont listés dans les observations.");
    }
    else
    {
        foreach (var feedback in export.Feedbacks)
        {
            builder.AppendLine($"- {feedback.Action} — {feedback.CreatedAt:u} — {feedback.Details}");
        }
    }
    builder.AppendLine();
    builder.AppendLine("## Audit pertinent");
    foreach (var audit in export.Audit.Take(20))
    {
        builder.AppendLine($"- {audit.Date:u} — {audit.Category}/{audit.Action} — {audit.Actor}");
    }
    builder.AppendLine();
    builder.AppendLine("## Limites / incertitudes");
    foreach (var limit in export.Limits)
    {
        builder.AppendLine($"- {limit}");
    }

    return builder.ToString();
}

static Guid StableGuid(string value)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("aegisloop-demo-v1:" + value));
    return new Guid(bytes.Take(16).ToArray());
}

static string StableSha256(string value)
{
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("aegisloop-demo-v1:" + value))).ToLowerInvariant();
}

public sealed record DemoStatusDto(string DatasetVersion, string SeedPath, bool Loaded, int Connectors, int RawItems, int Observations, int EventCases, int Scores, int Feedbacks, int AuditEntries, IReadOnlyList<string> Scenarios);
public sealed record DemoSeedFile(string DatasetVersion, DateTime GeneratedAt, string Description, IReadOnlyList<DemoConnectorSeed> Connectors, IReadOnlyList<DemoGroupSeed> Groups, IReadOnlyList<DemoFeedbackSeed> Feedbacks);
public sealed record DemoConnectorSeed(string Key, string Type, string Name, double Reliability, JsonElement Config);
public sealed record DemoGroupSeed(string Scenario, string ScenarioLabel, string Keyword, int Count, string CategoryHint, string TitleBase, string ContentBase, IReadOnlyList<string> ConnectorKeys, DemoLocationSeed? Location = null);
public sealed record DemoLocationSeed(string Name, string Region, string Country, double Latitude, double Longitude);
public sealed record DemoFeedbackSeed(int ObservationIndex, string Action, string Details);
public sealed record ExportMetadataDto(string Product, string Scope, string ExportVersion, DateTime ExportedAt, string Actor, Guid EventCaseId);
public sealed record EventCaseExportCaseDto(Guid Id, string Title, string? Summary, string Category, string Status, DateTime StartedAt, DateTime? EndedAt, int CorroborationCount, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record EventCaseExportDto(ExportMetadataDto Metadata, EventCaseExportCaseDto EventCase, ScoreBreakdownDto? Score, EventCaseProvenanceDto Provenance, IReadOnlyList<FeedbackDto> Feedbacks, IReadOnlyList<AuditEntryDto> Audit, IReadOnlyList<string> Limits);
public sealed record MapTimelineResponseDto(IReadOnlyList<MapTimelineItemDto> Items, int TotalCount, int WithoutCoordinatesCount, DateTime? PeriodStart, DateTime? PeriodEnd, IReadOnlyList<string> Scenarios, IReadOnlyList<string> SourceTypes);
public sealed record MapTimelineItemDto(Guid EventCaseId, string Title, double Score, string Status, string Category, DateTime Date, IReadOnlyList<string> Sources, IReadOnlyList<string> SourceTypes, int ObservationCount, double? Latitude, double? Longitude, string? LocationName, string? Region, string? Country, string? Scenario, string? ScenarioLabel);

// Rend Program accessible pour les tests d'intégration
public partial class Program { }