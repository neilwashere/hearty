using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

public class MessagePersistorTests
{
    /// <summary>
    /// This test verifies that the MessagePersistor can write messages to a file.
    /// It creates a temporary file, writes messages to it, and then reads them back to ensure they were persisted correctly.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task MessagePersistor_WritesMessagesToFile()
    {
        var tempFile = Path.GetTempFileName();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Hearty:PersistenceFilePath", tempFile }
            })
            .Build();

        var loggerMock = new Mock<ILogger<MessagePersistor>>();
        var channel = Channel.CreateUnbounded<TWWWSSMessage>();
        var writer = channel.Writer;
        var reader = channel.Reader;

        var persistor = new MessagePersistor(loggerMock.Object, reader, config);

        var testMessages = new[]
        {
            new TWWWSSMessage { Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = 100 },
            new TWWWSSMessage { Timestamp = DateTimeOffset.UtcNow.AddSeconds(1).ToUnixTimeMilliseconds(), Value = 200 }
        };

        var cts = new CancellationTokenSource();
        var persistorTask = persistor.StartAsync(cts.Token);

        foreach (var msg in testMessages)
        {
            await writer.WriteAsync(msg);
        }

        // Allow some time for persistor to process messages
        await Task.Delay(500);

        // Stop the persistor
        cts.Cancel();
        try { await persistorTask; } catch { /* ignore cancellation exceptions */ }

        var lines = File.ReadAllLines(tempFile);
        Assert.Equal(testMessages.Length, lines.Length);

        for (int i = 0; i < testMessages.Length; i++)
        {
            var deserialized = JsonSerializer.Deserialize<TWWWSSMessage>(lines[i]);
            Assert.Equal(testMessages[i].Timestamp, deserialized?.Timestamp);
            Assert.Equal(testMessages[i].Value, deserialized?.Value);
        }

        File.Delete(tempFile);
    }
}
