using System.Net;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using AegisLoop.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace AegisLoop.Api.Tests;

public sealed class ObservationsEndpointTests
{
    [Fact]
    public async Task GetObservations_Returns_Persisted_Observation()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            var connector = new SourceConnector { Name = $"RSS Test {Guid.NewGuid():N}", Status = AegisLoop.Domain.ConnectorStatus.Active };
            dbContext.SourceConnectors.Add(connector);
            dbContext.Observations.Add(new Observation
            {
                Title = "Observation API",
                Content = "Contenu API",
                SourceConnectorId = connector.Id,
                ObservedAt = new DateTime(2026, 04, 28, 10, 00, 00, DateTimeKind.Utc)
            });
            await dbContext.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/observations");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Observation API", body);
        Assert.Contains("RSS Test", body);
    }

    [Fact]
    public async Task GetConnectors_Returns_Default_Rss_And_Gdelt_Connectors()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/connectors");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("RSS Démo", body);
        Assert.Contains("GDELT Démo", body);
    }

    [Fact]
    public async Task GetIngestionJobs_Returns_Persisted_Jobs()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            var connector = dbContext.SourceConnectors.First(c => c.ConnectorType == ConnectorType.Rss);
            dbContext.IngestionJobs.Add(new IngestionJob
            {
                ConnectorId = connector.Id,
                StartedAt = new DateTime(2026, 04, 28, 10, 00, 00, DateTimeKind.Utc),
                CompletedAt = new DateTime(2026, 04, 28, 10, 01, 00, DateTimeKind.Utc),
                Status = JobStatus.Completed,
                ItemsCollected = 2,
                ItemsNormalized = 2
            });
            await dbContext.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/ingestion/jobs");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Completed", body);
        Assert.Contains("itemsCollected", body);
    }

    [Fact]
    public async Task GetDashboard_Returns_Real_Counters()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            var connector = dbContext.SourceConnectors.First(c => c.ConnectorType == ConnectorType.Gdelt);
            dbContext.RawItems.Add(new RawItem
            {
                ConnectorId = connector.Id,
                SourceHash = Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLowerInvariant() + Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLowerInvariant(),
                RawContent = "{\"source\":\"gdelt\",\"title\":\"Dashboard raw\"}",
                ContentType = RawContentType.Json
            });
            dbContext.Observations.Add(new Observation
            {
                Title = "Dashboard observation",
                Content = "Contenu",
                SourceConnectorId = connector.Id,
                ObservedAt = DateTime.UtcNow
            });
            dbContext.IngestionJobs.Add(new IngestionJob
            {
                ConnectorId = connector.Id,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                Status = JobStatus.Completed,
                ItemsCollected = 1,
                ItemsNormalized = 1
            });
            await dbContext.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/dashboard");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("connectors", body);
        Assert.Contains("rawItems", body);
        Assert.Contains("observations", body);
        Assert.Contains("lastIngestion", body);
    }

    [Fact]
    public async Task Events_Rebuild_List_Detail_And_ScoringBreakdown_Return_Real_Data()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            var rss = dbContext.SourceConnectors.First(c => c.ConnectorType == ConnectorType.Rss);
            var gdelt = dbContext.SourceConnectors.First(c => c.ConnectorType == ConnectorType.Gdelt);
            dbContext.Observations.AddRange(
                new Observation
                {
                    Title = "Khartoum conflict incident",
                    Content = "Khartoum conflict incident reported downtown",
                    ClaimText = "Khartoum conflict incident reported downtown",
                    SourceConnectorId = rss.Id,
                    SourceReliability = 0.8,
                    ObservedAt = new DateTime(2026, 04, 28, 10, 00, 00, DateTimeKind.Utc)
                },
                new Observation
                {
                    Title = "Khartoum conflict incident update",
                    Content = "Another source confirms Khartoum conflict incident",
                    ClaimText = "Another source confirms Khartoum conflict incident",
                    SourceConnectorId = gdelt.Id,
                    SourceReliability = 0.6,
                    ObservedAt = new DateTime(2026, 04, 28, 11, 00, 00, DateTimeKind.Utc)
                });
            await dbContext.SaveChangesAsync();
        }

        var rebuild = await client.PostAsync("/api/events/rebuild", null);
        var rebuildBody = await rebuild.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, rebuild.StatusCode);
        Assert.Contains("eventCasesCreated", rebuildBody);

        var list = await client.GetAsync("/api/events");
        var listBody = await list.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Contains("Khartoum conflict incident", listBody);
        Assert.Contains("scoreBreakdown", listBody);

        Guid eventId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            eventId = dbContext.EventCases.Select(e => e.Id).First();
        }

        var detail = await client.GetAsync($"/api/events/{eventId}");
        var detailBody = await detail.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.Contains("observations", detailBody);
        Assert.Contains("SourceReliability", detailBody);

        var breakdown = await client.GetAsync($"/api/scoring/{eventId}/breakdown");
        var breakdownBody = await breakdown.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, breakdown.StatusCode);
        Assert.Contains("Corroboration", breakdownBody);
        Assert.Contains("AnalystFeedback", breakdownBody);
    }

    [Fact]
    public async Task Dashboard_Returns_EventCase_Enrichment()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            var connector = dbContext.SourceConnectors.First(c => c.ConnectorType == ConnectorType.Rss);
            var eventCase = new EventCase
            {
                Title = "Dashboard EventCase",
                Category = EventCategory.Conflict,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CorroborationCount = 1
            };
            dbContext.EventCases.Add(eventCase);
            dbContext.Observations.Add(new Observation
            {
                Title = "Dashboard EventCase Observation",
                Content = "Dashboard EventCase Observation content",
                SourceConnectorId = connector.Id,
                EventCaseId = eventCase.Id,
                ObservedAt = DateTime.UtcNow
            });
            dbContext.ConfidenceScores.Add(new ConfidenceScore
            {
                TargetId = eventCase.Id,
                TargetType = ScoreTargetType.EventCase,
                Value = 0.75,
                SourceReliability = 0.7,
                Corroboration = 0.33,
                AnalystFeedback = 0.5
            });
            await dbContext.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/dashboard");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("recentEvents", body);
        Assert.Contains("highScoreEvents", body);
        Assert.Contains("categoryDistribution", body);
        Assert.Contains("sourceDistribution", body);
        Assert.Contains("Dashboard EventCase", body);
    }

    [Fact]
    public async Task Observation_Provenance_Returns_Real_Source_RawItem_Job_Score_And_Feedback()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        Guid observationId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            var connector = dbContext.SourceConnectors.First(c => c.ConnectorType == ConnectorType.Rss);
            var job = new IngestionJob { ConnectorId = connector.Id, StartedAt = new DateTime(2026, 04, 28, 09, 59, 00, DateTimeKind.Utc), CompletedAt = new DateTime(2026, 04, 28, 10, 01, 00, DateTimeKind.Utc), Status = JobStatus.Completed, ItemsCollected = 1, ItemsNormalized = 1 };
            var raw = new RawItem { ConnectorId = connector.Id, SourceHash = Hash(), RawContent = "{\"source\":\"rss\",\"feedTitle\":\"Feed Test\",\"title\":\"Prov raw\",\"link\":\"https://example.test/prov\"}", ContentType = RawContentType.Json, CollectedAt = new DateTime(2026, 04, 28, 10, 00, 00, DateTimeKind.Utc), SourceUrl = "https://example.test/prov" };
            var observation = new Observation { RawItemId = raw.Id, Title = "Observation provenance", Content = "Contenu", SourceConnectorId = connector.Id, SourceUrl = raw.SourceUrl, ObservedAt = raw.CollectedAt, MetadataJson = "{\"source\":\"rss\",\"sourceHash\":\"" + raw.SourceHash + "\"}" };
            dbContext.IngestionJobs.Add(job);
            dbContext.RawItems.Add(raw);
            dbContext.Observations.Add(observation);
            dbContext.ConfidenceScores.Add(new ConfidenceScore { TargetId = observation.Id, TargetType = ScoreTargetType.Observation, Value = 0.61, SourceReliability = 0.35, Corroboration = 0.33, AnalystFeedback = 0.5 });
            dbContext.AnalystFeedbacks.Add(new AnalystFeedback { TargetId = observation.Id, TargetType = nameof(Observation), Action = FeedbackAction.Note, Details = "note test" });
            await dbContext.SaveChangesAsync();
            observationId = observation.Id;
        }

        var response = await client.GetAsync($"/api/observations/{observationId}/provenance");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Observation provenance", body);
        Assert.Contains("sourceConnector", body);
        Assert.Contains("rawItem", body);
        Assert.Contains("sourceHash", body);
        Assert.Contains("ingestionJob", body);
        Assert.Contains("Feed Test", body);
        Assert.Contains("AnalystFeedback", body);
        Assert.DoesNotContain("\"id\":\"" + observationId, body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Feedback_Observation_Is_Persisted_Audited_And_Recalculates_Score()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        Guid observationId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            var connector = dbContext.SourceConnectors.First(c => c.ConnectorType == ConnectorType.Rss);
            var observation = new Observation { Title = "Feedback observation", Content = "Contenu", SourceConnectorId = connector.Id, SourceReliability = 0.35, ObservedAt = DateTime.UtcNow };
            dbContext.Observations.Add(observation);
            await dbContext.SaveChangesAsync();
            observationId = observation.Id;
        }

        var response = await client.PostAsJsonAsync("/api/feedback", new { targetId = observationId, targetType = "Observation", action = "Confirm", details = "validé" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Confirm", body);
        Assert.Contains("AnalystFeedback", body);
        using var scope2 = factory.Services.CreateScope();
        var dbContext2 = scope2.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
        Assert.True(dbContext2.AnalystFeedbacks.Any(f => f.TargetId == observationId && f.Action == FeedbackAction.Confirm));
        Assert.True(dbContext2.AuditEntries.Any(a => a.TargetId == observationId && a.Action == "AnalystFeedbackSubmitted"));
        Assert.True(dbContext2.ConfidenceScores.Any(s => s.TargetId == observationId && s.AnalystFeedback == 1.0));
    }

    [Fact]
    public async Task Feedback_EventCase_And_Event_Provenance_Are_Real_And_Audited()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        Guid eventId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            var connector = dbContext.SourceConnectors.First(c => c.ConnectorType == ConnectorType.Gdelt);
            var eventCase = new EventCase { Title = "Event provenance", Category = EventCategory.Conflict, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var raw = new RawItem { ConnectorId = connector.Id, SourceHash = Hash(), RawContent = "{\"source\":\"gdelt\",\"domain\":\"example.test\",\"title\":\"GDELT raw\"}", ContentType = RawContentType.Json, CollectedAt = DateTime.UtcNow };
            var observation = new Observation { RawItemId = raw.Id, EventCaseId = eventCase.Id, Title = "Event observation", Content = "Contenu", SourceConnectorId = connector.Id, ObservedAt = DateTime.UtcNow };
            dbContext.EventCases.Add(eventCase);
            dbContext.RawItems.Add(raw);
            dbContext.Observations.Add(observation);
            dbContext.ConfidenceScores.Add(new ConfidenceScore { TargetId = observation.Id, TargetType = ScoreTargetType.Observation, Value = 0.5, AnalystFeedback = 0.5 });
            dbContext.ConfidenceScores.Add(new ConfidenceScore { TargetId = eventCase.Id, TargetType = ScoreTargetType.EventCase, Value = 0.4, AnalystFeedback = 0.5 });
            await dbContext.SaveChangesAsync();
            eventId = eventCase.Id;
        }

        var feedback = await client.PostAsJsonAsync("/api/feedback", new { targetId = eventId, targetType = "EventCase", action = "Invalidate", details = "doublon" });
        Assert.Equal(HttpStatusCode.OK, feedback.StatusCode);

        var provenance = await client.GetAsync($"/api/events/{eventId}/provenance");
        var body = await provenance.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, provenance.StatusCode);
        Assert.Contains("Event provenance", body);
        Assert.Contains("Event observation", body);
        Assert.Contains("hashes", body);
        Assert.Contains("Invalidate", body);
        Assert.Contains("domain", body);
    }

    [Fact]
    public async Task Audit_Endpoint_Returns_Useful_Audit_Entries()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            dbContext.AuditEntries.Add(new AuditEntry { Category = AuditCategory.Analyst, Action = "AuditEndpointTest", Actor = "test", TargetType = nameof(Observation), TargetId = Guid.NewGuid(), Details = "message test" });
            await dbContext.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/audit?take=10");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("AuditEndpointTest", body);
        Assert.Contains("category", body);
        Assert.Contains("targetType", body);
        Assert.Contains("level", body);
        Assert.Contains("message test", body);
    }

    [Fact]
    public async Task Demo_Load_Reset_Rebuild_Recalculate_Are_Audited_And_Idempotent()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var load = await client.PostAsync("/api/demo/load", null);
        var loadBody = await load.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, load.StatusCode);
        Assert.Contains("v1-seed-2026-04", loadBody);

        var status = await client.GetAsync("/api/demo/status");
        var statusBody = await status.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.Contains("\"observations\":90", statusBody);
        Assert.Contains("\"eventCases\":8", statusBody);
        Assert.Contains("sahel-civic-security", statusBody);
        Assert.Contains("aden-maritime-incident", statusBody);

        var reload = await client.PostAsync("/api/demo/load", null);
        Assert.Equal(HttpStatusCode.OK, reload.StatusCode);
        var statusAfterReload = await client.GetAsync("/api/demo/status");
        var statusAfterReloadBody = await statusAfterReload.Content.ReadAsStringAsync();
        Assert.Contains("\"observations\":90", statusAfterReloadBody);

        var rebuild = await client.PostAsync("/api/demo/rebuild", null);
        var rebuildBody = await rebuild.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, rebuild.StatusCode);
        Assert.Contains("eventCasesCreated", rebuildBody);

        var recalculate = await client.PostAsync("/api/demo/recalculate", null);
        Assert.Equal(HttpStatusCode.OK, recalculate.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            Assert.Equal(90, dbContext.Observations.Count(o => o.MetadataJson != null && o.MetadataJson.Contains("v1-seed-2026-04")));
            Assert.Equal(8, dbContext.EventCases.Count());
            Assert.True(dbContext.AuditEntries.Any(a => a.Action == "DemoSeedLoaded"));
            Assert.True(dbContext.AuditEntries.Any(a => a.Action == "DemoEventCasesRebuilt"));
            Assert.True(dbContext.AuditEntries.Any(a => a.Action == "DemoScoresRecalculated"));
        }

        var reset = await client.PostAsync("/api/demo/reset", null);
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        using var resetScope = factory.Services.CreateScope();
        var resetDb = resetScope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
        Assert.Empty(resetDb.Observations);
        Assert.True(resetDb.AuditEntries.Any(a => a.Action == "DemoReset"));
    }

    [Fact]
    public async Task MapTimeline_Returns_Seed_EventCases_With_Geo_Timeline_And_Filters()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var load = await client.PostAsync("/api/demo/load", null);
        Assert.Equal(HttpStatusCode.OK, load.StatusCode);

        var response = await client.GetAsync("/api/map-timeline");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"items\"", body);
        Assert.Contains("Noria", body);
        Assert.Contains("Bab el Mandeb", body);
        Assert.Contains("\"latitude\"", body);
        Assert.Contains("\"longitude\"", body);
        Assert.Contains("\"withoutCoordinatesCount\":0", body);
        Assert.Contains("aden-maritime-incident", body);
        Assert.Contains("sahel-civic-security", body);

        using (var document = JsonDocument.Parse(body))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(8, data.GetProperty("totalCount").GetInt32());
            Assert.Equal(8, data.GetProperty("items").GetArrayLength());
            Assert.NotNull(data.GetProperty("periodStart").GetString());
            Assert.NotNull(data.GetProperty("periodEnd").GetString());
        }

        var filtered = await client.GetAsync("/api/map-timeline?source=Gdelt&scenario=aden-maritime-incident&minScore=0.1");
        var filteredBody = await filtered.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, filtered.StatusCode);
        Assert.Contains("aden-maritime-incident", filteredBody);
        Assert.DoesNotContain("sahel-civic-security", filteredBody);
        Assert.Contains("Gdelt", filteredBody);
    }

    [Fact]
    public async Task Export_EventCase_Json_And_Markdown_Return_Complete_Content_And_Audit()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        await client.PostAsync("/api/demo/load", null);

        Guid eventId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
            eventId = dbContext.EventCases.OrderBy(e => e.StartedAt).Select(e => e.Id).First();
        }

        var json = await client.GetAsync($"/api/export/{eventId}?format=json");
        var jsonBody = await json.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, json.StatusCode);
        Assert.Contains("\"metadata\"", jsonBody);
        Assert.Contains("\"eventCase\"", jsonBody);
        Assert.Contains("\"score\"", jsonBody);
        Assert.Contains("\"provenance\"", jsonBody);
        Assert.Contains("\"feedbacks\"", jsonBody);
        Assert.Contains("\"audit\"", jsonBody);
        Assert.Contains("\"limits\"", jsonBody);

        using var document = JsonDocument.Parse(jsonBody);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.True(document.RootElement.GetProperty("data").GetProperty("provenance").GetProperty("observations").GetArrayLength() > 0);

        var markdown = await client.GetAsync($"/api/export/{eventId}?format=markdown");
        var markdownBody = await markdown.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, markdown.StatusCode);
        Assert.Contains("# ", markdownBody);
        Assert.Contains("## Résumé", markdownBody);
        Assert.Contains("## Score", markdownBody);
        Assert.Contains("## Sources", markdownBody);
        Assert.Contains("## Observations", markdownBody);
        Assert.Contains("## Provenance", markdownBody);
        Assert.Contains("## Feedback analyste", markdownBody);
        Assert.Contains("## Limites / incertitudes", markdownBody);

        using var auditScope = factory.Services.CreateScope();
        var auditDb = auditScope.ServiceProvider.GetRequiredService<AegisLoopDbContext>();
        Assert.True(auditDb.AuditEntries.Any(a => a.Action == "EventCaseExported" && a.TargetId == eventId));
    }

    private sealed class TestApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }
    }

    private static string Hash()
    {
        return Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLowerInvariant()
            + Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLowerInvariant();
    }
}