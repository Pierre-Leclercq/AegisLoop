using Xunit;
using AegisLoop.Infrastructure.Data;

namespace AegisLoop.Infrastructure.Tests;

public class SmokeTests
{
    [Fact]
    public void Infrastructure_Project_Loads_Successfully()
    {
        Assert.Contains(nameof(AegisLoopDbContext.RawItems), typeof(AegisLoopDbContext).GetProperties().Select(p => p.Name));
        Assert.Contains(nameof(AegisLoopDbContext.IngestionJobs), typeof(AegisLoopDbContext).GetProperties().Select(p => p.Name));
    }
}