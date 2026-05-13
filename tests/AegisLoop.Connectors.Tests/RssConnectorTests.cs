using System.Net;
using AegisLoop.Connectors;
using Xunit;

namespace AegisLoop.Connectors.Tests;

public sealed class RssConnectorTests
{
    [Fact]
    public async Task CollectAsync_Parses_Rss_Fixture_Into_RawItems()
    {
        var connector = new RssConnector(new HttpClient(new StubHttpHandler(RssFixture())));

        var result = await connector.CollectAsync("""
            { "feedUrl": "https://example.test/rss.xml", "maxItemsPerPoll": 10 }
            """, since: null);

        Assert.True(result.Success);
        Assert.Equal(2, result.ItemsCollected);
        Assert.All(result.Items, item => Assert.False(string.IsNullOrWhiteSpace(item.SourceHash)));
        Assert.Contains("Article Alpha", result.Items[0].RawContent);
        Assert.Equal("https://example.test/alpha", result.Items[0].SourceUrl);
    }

    [Fact]
    public async Task CollectAsync_Generates_Stable_SourceHash()
    {
        var connector = new RssConnector(new HttpClient(new StubHttpHandler(RssFixture())));
        var config = "{ \"feedUrl\": \"https://example.test/rss.xml\" }";

        var first = await connector.CollectAsync(config, null);
        var second = await connector.CollectAsync(config, null);

        Assert.Equal(first.Items[0].SourceHash, second.Items[0].SourceHash);
        Assert.Equal(RssConnector.ComputeSourceHash("https://example.test/alpha", "Article Alpha", new DateTime(2026, 04, 28, 10, 00, 00, DateTimeKind.Utc)), first.Items[0].SourceHash);
    }

    [Fact]
    public async Task ValidateConfigAsync_Rejects_Invalid_FeedUrl()
    {
        var connector = new RssConnector(new HttpClient(new StubHttpHandler(RssFixture())));

        var result = await connector.ValidateConfigAsync("{ \"feedUrl\": \"file:///tmp/rss.xml\" }");

        Assert.False(result.IsValid);
        Assert.Contains("HTTP", result.ErrorMessage);
    }

    [Fact]
    public async Task CollectAsync_Returns_Error_For_Invalid_Xml()
    {
        var connector = new RssConnector(new HttpClient(new StubHttpHandler("not xml")));

        var result = await connector.CollectAsync("{ \"feedUrl\": \"https://example.test/rss.xml\" }", null);

        Assert.False(result.Success);
        Assert.Contains("invalide", result.Errors[0]);
    }

    private static string RssFixture() => """
        <?xml version="1.0" encoding="utf-8" ?>
        <rss version="2.0">
          <channel>
            <title>Flux Test</title>
            <language>fr</language>
            <item>
              <title>Article Alpha</title>
              <link>https://example.test/alpha</link>
              <pubDate>Tue, 28 Apr 2026 10:00:00 GMT</pubDate>
              <description><![CDATA[Résumé <b>Alpha</b>]]></description>
            </item>
            <item>
              <title>Article Beta</title>
              <link>https://example.test/beta</link>
              <description>Résumé Beta sans date</description>
            </item>
          </channel>
        </rss>
        """;

    private sealed class StubHttpHandler : HttpMessageHandler
    {
        private readonly string _content;
        private readonly HttpStatusCode _statusCode;

        public StubHttpHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _content = content;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            });
        }
    }
}

public sealed class GdeltConnectorTests
{
    [Fact]
    public async Task CollectAsync_Parses_Gdelt_Fixture_Into_RawItems()
    {
        var connector = new GdeltConnector(new HttpClient(new StubHttpHandler(GdeltFixture())));

        var result = await connector.CollectAsync("""
            { "query": "Sudan conflict", "maxItemsPerPoll": 10 }
            """, since: null);

        Assert.True(result.Success);
        Assert.Equal(2, result.ItemsCollected);
        Assert.All(result.Items, item => Assert.False(string.IsNullOrWhiteSpace(item.SourceHash)));
        Assert.Contains("gdelt", result.Items[0].RawContent);
        Assert.Equal("https://example.test/gdelt-alpha", result.Items[0].SourceUrl);
    }

    [Fact]
    public async Task CollectAsync_Generates_Stable_Gdelt_SourceHash()
    {
        var connector = new GdeltConnector(new HttpClient(new StubHttpHandler(GdeltFixture())));
        var config = "{ \"query\": \"Sudan conflict\" }";

        var first = await connector.CollectAsync(config, null);
        var second = await connector.CollectAsync(config, null);

        var expected = GdeltConnector.ComputeSourceHash(
            "https://example.test/gdelt-alpha",
            "GDELT Alpha Incident",
            new DateTime(2026, 04, 28, 10, 15, 00, DateTimeKind.Utc));
        Assert.Equal(first.Items[0].SourceHash, second.Items[0].SourceHash);
        Assert.Equal(expected, first.Items[0].SourceHash);
    }

    [Fact]
    public async Task CollectAsync_Returns_Error_For_Invalid_Json()
    {
        var connector = new GdeltConnector(new HttpClient(new StubHttpHandler("not json")));

        var result = await connector.CollectAsync("{ \"query\": \"Sudan conflict\" }", null);

        Assert.False(result.Success);
        Assert.Contains("JSON", result.Errors[0]);
    }

    private static string GdeltFixture() => """
        {
          "articles": [
            {
              "url": "https://example.test/gdelt-alpha",
              "title": "GDELT Alpha Incident",
              "seendate": "20260428T101500Z",
              "domain": "example.test",
              "language": "English",
              "sourcecountry": "United States"
            },
            {
              "url": "https://example.test/gdelt-beta",
              "title": "GDELT Beta Update",
              "seendate": "20260428T102000Z",
              "domain": "example.test",
              "language": "English",
              "sourcecountry": "United Kingdom"
            }
          ]
        }
        """;

    private sealed class StubHttpHandler : HttpMessageHandler
    {
        private readonly string _content;
        private readonly HttpStatusCode _statusCode;

        public StubHttpHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _content = content;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Contains("api/v2/doc/doc", request.RequestUri?.AbsoluteUri);
            Assert.Contains("format=json", request.RequestUri?.Query);
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            });
        }
    }
}
