using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing;

public class HistoricalPageIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HistoricalPageIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HistoricalPage_LoadsAndRendersChartData()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Historical?TimeValue=5&TimeUnit=minutes");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Parse HTML and check for chartData JS variable
        var parser = new HtmlParser();
        var document = parser.ParseDocument(content);

        // Look for the chart invocation
        var scriptContent = document.QuerySelectorAll("script")
            .Select(s => s.TextContent)
            .FirstOrDefault(s => s.Contains("initialiseHistoricalChart"));

        Assert.False(string.IsNullOrWhiteSpace(scriptContent));

        // Optionally, check that the chart container exists
        Assert.NotNull(document.QuerySelector("#historicalChart"));
    }
}
