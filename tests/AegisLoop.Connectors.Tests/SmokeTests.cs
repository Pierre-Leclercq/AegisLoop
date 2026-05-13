using Xunit;

namespace AegisLoop.Connectors.Tests;

public class SmokeTests
{
    [Fact]
    public void Connectors_Project_Loads_Successfully()
    {
        Assert.Equal("Rss", new RssConnector(new HttpClient()).ConnectorType);
        Assert.Equal("Gdelt", new GdeltConnector(new HttpClient()).ConnectorType);
    }
}