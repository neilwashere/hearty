using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Hearty.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        var factoryWithConfig = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json");
            });
        });

        client = factoryWithConfig.CreateClient();
    }

    /// <summary>
    /// This is a pretty basic integration test that verifies the /messages endpoint just returns a list.
    /// TODO: mock the message retriever to return a known set of messages
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task MessagesEndpoint_ReturnsOkAndList()
    {
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
    }
}
