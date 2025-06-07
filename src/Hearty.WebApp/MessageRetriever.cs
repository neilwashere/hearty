using System.Text.Json;

public class MessageRetriever(ILogger<MessageRetriever> logger) 
{
    private const string InputFileName = "hearty_data.log";

    // Reads the file and returns data within the specified date range.
    public IEnumerable<string> ReadByDateRange(DateTime start, DateTime end)
    {
        // Convert the start and end DateTime to Unix epoch milliseconds.
        long startTimestamp = new DateTimeOffset(start).ToUnixTimeMilliseconds();
        long endTimestamp = new DateTimeOffset(end).ToUnixTimeMilliseconds();

        logger.LogInformation("⌛ Retrieving messages from {Start} to {End} (timestamps: {StartTimestamp} to {EndTimestamp})", 
            start, end, startTimestamp, endTimestamp);

        // Use an iterator to efficiently yield matching lines.
        // Allow the caller to figure out how to present the data (eg. wrapping into an array).
        foreach (var line in File.ReadLines(InputFileName))
        {
            bool shouldYield = false;
            try
            {
                var record = JsonSerializer.Deserialize<TWWWSSMessage>(line);

                // Check if the timestamp is within the specified range.
                if (record != null && record.Timestamp >= startTimestamp && record.Timestamp <= endTimestamp)
                {
                    shouldYield = true;
                }
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "💥 Invalid JSON line: {Line} from persistence!", line);
            }
            if (shouldYield)
            {
                yield return line;
            }
        }
    }
}