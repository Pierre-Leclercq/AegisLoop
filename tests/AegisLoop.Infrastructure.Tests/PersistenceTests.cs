using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using AegisLoop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AegisLoop.Infrastructure.Tests;

public sealed class PersistenceTests
{
    [Fact]
    public async Task Store_Persists_RawItem_Observation_And_Detects_Duplicate_SourceHash()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        var store = new EfAegisLoopStore(dbContext);

        var connector = await store.EnsureDemoRssConnectorAsync("https://example.test/rss.xml");
        var rawItem = new RawItem
        {
            ConnectorId = connector.Id,
            ContentType = RawContentType.Json,
            SourceHash = new string('c', 64),
            RawContent = "{\"title\":\"Persisted\",\"summary\":\"Content\"}"
        };

        Assert.False(await store.SourceHashExistsAsync(rawItem.SourceHash));
        await store.AddRawItemAsync(rawItem);
        Assert.True(await store.SourceHashExistsAsync(rawItem.SourceHash));

        await store.AddObservationAsync(new Observation
        {
            RawItemId = rawItem.Id,
            SourceConnectorId = connector.Id,
            Title = "Persisted",
            Content = "Content",
            ObservedAt = DateTime.UtcNow
        });

        var observations = await store.ListObservationsAsync();
        Assert.Single(observations);
        Assert.Equal("RSS Démo", observations[0].SourceName);
    }

    [Fact]
    public async Task Store_Writes_Audit_And_IngestionJob()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        var store = new EfAegisLoopStore(dbContext);

        var connector = await store.EnsureDemoRssConnectorAsync("https://example.test/rss.xml");
        var job = await store.CreateIngestionJobAsync(connector.Id);
        await store.AddAuditEntryAsync(new AuditEntry
        {
            Category = AuditCategory.Ingestion,
            Action = "IngestionStarted",
            Actor = "test",
            TargetType = nameof(SourceConnector),
            TargetId = connector.Id
        });
        await store.CompleteIngestionJobAsync(job.Id, JobStatus.Completed, 2, 2, null);

        Assert.Single(await store.ListIngestionJobsAsync());
        Assert.Equal(1, await dbContext.AuditEntries.CountAsync());
    }

    [Fact]
    public async Task Store_Persists_Common_SourceHash_Deduplication_For_Rss_And_Gdelt()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        var store = new EfAegisLoopStore(dbContext);

        var rss = await store.EnsureDemoRssConnectorAsync("https://example.test/rss.xml");
        var gdelt = await store.EnsureDemoGdeltConnectorAsync("Sudan conflict");
        var duplicateHash = new string('e', 64);

        await store.AddRawItemAsync(new RawItem
        {
            ConnectorId = rss.Id,
            ContentType = RawContentType.Json,
            SourceHash = duplicateHash,
            RawContent = "{\"title\":\"RSS duplicate\"}"
        });

        Assert.True(await store.SourceHashExistsAsync(duplicateHash));
        Assert.NotEqual(rss.Id, gdelt.Id);
    }

    private static AegisLoopDbContext CreateDbContext()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"aegisloop-test-{Guid.NewGuid():N}.db");
        return new AegisLoopDbContext(new DbContextOptionsBuilder<AegisLoopDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options);
    }
}
