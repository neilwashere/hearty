using System.Threading.Channels;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public class TWWWSSMessageHandlerTests
{
    /// <summary>
    /// This test verifies that the TWWWSSMessageHandler can handle a valid message
    /// and writes it to the channel. It simulates receiving a message in JSON format,
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task HandleMessageAsync_ValidMessage_WritesToChannel()
    {
        var logger = new LoggerFactory().CreateLogger<TWWWSSMessageHandler>();
        var channel = Channel.CreateUnbounded<TWWWSSMessage>();
        var handler = new TWWWSSMessageHandler(logger, channel.Writer);

        var testMessage = new TWWWSSMessage
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Value = 123
        };
        var json = JsonSerializer.Serialize(testMessage);

        await handler.HandleMessageAsync(json);

        Assert.True(await channel.Reader.WaitToReadAsync());
        var result = await channel.Reader.ReadAsync();
        Assert.Equal(testMessage.Timestamp, result.Timestamp);
        Assert.Equal(testMessage.Value, result.Value);
    }
}
