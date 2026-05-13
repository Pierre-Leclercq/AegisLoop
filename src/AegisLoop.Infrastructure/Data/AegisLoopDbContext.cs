using AegisLoop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AegisLoop.Infrastructure.Data;

/// <summary>
/// DbContext SQLite pour AegisLoop V1 — 11 types du domaine.
/// </summary>
public class AegisLoopDbContext : DbContext
{
    public AegisLoopDbContext(DbContextOptions<AegisLoopDbContext> options) : base(options) { }

    // 11 types V1
    public DbSet<SourceConnector> SourceConnectors => Set<SourceConnector>();
    public DbSet<RawItem> RawItems => Set<RawItem>();
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<EventCase> EventCases => Set<EventCase>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Contradiction> Contradictions => Set<Contradiction>();
    public DbSet<ConfidenceScore> ConfidenceScores => Set<ConfidenceScore>();
    public DbSet<AnalystFeedback> AnalystFeedbacks => Set<AnalystFeedback>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<IngestionJob> IngestionJobs => Set<IngestionJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SourceConnector>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Config).IsRequired();
            entity.Property(e => e.ConnectorType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasIndex(e => new { e.ConnectorType, e.Name }).IsUnique();
        });

        modelBuilder.Entity<RawItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.ContentType).HasConversion<string>();
            entity.HasIndex(e => e.SourceHash).IsUnique();
        });

        modelBuilder.Entity<Observation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasIndex(e => e.ObservedAt);
            entity.HasIndex(e => e.SourceConnectorId);
            entity.HasIndex(e => e.EventCaseId);
        });

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            entity.Property(e => e.NormalizedName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasIndex(e => e.NormalizedName);
        });

        modelBuilder.Entity<EventCase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasIndex(e => e.UpdatedAt);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
        });

        modelBuilder.Entity<Contradiction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<ConfidenceScore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetType).HasConversion<string>();
            entity.HasIndex(e => new { e.TargetId, e.TargetType, e.CalculatedAt });
        });

        modelBuilder.Entity<AnalystFeedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasConversion<string>();
        });

        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasConversion<string>();
            entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Actor).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<IngestionJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
        });
    }
}