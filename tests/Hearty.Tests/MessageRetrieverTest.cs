using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

public class MessageRetrieverTests
{
    private static string CreateTestLogFile(params TWWWSSMessage[] messages)
    {
        var tempPath = Path.GetTempFileName();
        using var writer = new StreamWriter(tempPath);
        foreach (var msg in messages)
        {
            writer.WriteLine(JsonSerializer.Serialize(msg));
        }
        return tempPath;
    }

    private static IConfiguration CreateConfig(string logPath)
    {
        var dict = new Dictionary<string, string?>
        {
            { "Hearty:PersistenceFilePath", logPath }
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    /// <summary>
    /// Not incredibly resilient but this will test that the MessageRetriever can read messages and will only
    /// return those that fall within the specified date range.
    /// </summary>
    [Fact]
    public void ReadByDateRange_ReturnsMatchingMessages()
    {
        var now = DateTime.UtcNow;
        // This message is outside of the test range so should not be returned
        var msg1 = new TWWWSSMessage { Timestamp = new DateTimeOffset(now.AddMinutes(-10)).ToUnixTimeMilliseconds(), Value = 1 };
        // These messages are within the test range and should be returned
        var msg2 = new TWWWSSMessage { Timestamp = new DateTimeOffset(now.AddMinutes(-5)).ToUnixTimeMilliseconds(), Value = 2 };
        var msg3 = new TWWWSSMessage { Timestamp = new DateTimeOffset(now.AddMinutes(-1)).ToUnixTimeMilliseconds(), Value = 3 };

        // Create a temporary log file with the test messages var
        var logPath = CreateTestLogFile(msg1, msg2, msg3);

        // Setup our retriever
        var config = CreateConfig(logPath);
        var loggerMock = new Mock<ILogger<MessageRetriever>>().Object;
        var retriever = new MessageRetriever(loggerMock, config);

        var start = now.AddMinutes(-6);
        var end = now;
        var results = retriever.ReadByDateRange(start, end).ToList();

        Assert.Single(results, m => m != null && m.Value == 2);
        Assert.Single(results, m => m != null && m.Value == 3);
        Assert.DoesNotContain(results, m => m != null && m.Value == 1);

        File.Delete(logPath);
    }

}
