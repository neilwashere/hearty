using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

public class SignalRStreamIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SignalRStreamIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json");
            });
        });
    }

    /// <summary>
    /// This is a very ugly integration test that verifies the SignalR stream endpoint
    /// can be connected to and that it receives data from the live TWWWSS debug stream.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SignalR_Stream_ReceivesData()
    {
        // Arrange
        var client = _factory.Server.CreateClient();
        var serverUri = client.BaseAddress?.ToString().TrimEnd('/');
        var hubUrl = $"{serverUri}/stream";

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();

        await connection.StartAsync();

        var receivedMessages = new List<TWWWSSMessage>();
        var stream = connection.StreamAsync<TWWWSSMessage>("StreamData");

        try {
            // TODO - there is a better way to handle this as the canellation token blows up on exit
            // I just want this to run for a few seconds and then stop
            await foreach (var msg in stream.WithCancellation(new CancellationTokenSource(4000).Token))
            {
                receivedMessages.Add(msg);
            }
        } catch (Exception)
        {
            // We expect this to blow up
        }

        await connection.StopAsync();

        // Assert
        Assert.NotEmpty(receivedMessages);
    }

}
