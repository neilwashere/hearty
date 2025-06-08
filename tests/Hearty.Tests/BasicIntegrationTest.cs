using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Hearty.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MessagesEndpoint_ReturnsOkAndList()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Use a date range that should work (adjust as needed)
        var start = DateTime.UtcNow.AddMinutes(-10).ToString("o");
        var end = DateTime.UtcNow.ToString("o");

        var url = $"/messages?start={WebUtility.UrlEncode(start)}&end={WebUtility.UrlEncode(end)}";

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var messages = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(messages);
        // Optionally, assert on message content if you have test data
    }
}
