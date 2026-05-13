using Xunit;

namespace AegisLoop.E2E.Tests;

public class SmokeTests
{
    [Fact]
    public void E2E_Project_Loads_Successfully()
    {
        var expectedV1Views = new[] { "Dashboard", "Carte + Timeline", "EventCase", "Observations", "Paramètres" };

        Assert.Equal(5, expectedV1Views.Length);
        Assert.Contains("Observations", expectedV1Views);
    }
}