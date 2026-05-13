using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace AegisLoop.Api.Tests;

public class SmokeTests
{
    [Fact]
    public async Task Health_Endpoint_Returns_Success()
    {
        await using var factory = new HealthApiFactory();
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.True(response.IsSuccessStatusCode);
    }

    private sealed class HealthApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }
    }
}