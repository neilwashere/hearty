using System.Text.Json;

public interface IMessageRetriever
{
    IEnumerable<string> ReadByDateRange(DateTime start, DateTime end);
}

/// <summary>
/// This class retrieves messages from a file within a specified date range.
/// It reads the file line by line, deserializes each line into a TWWWSSMessage object,
/// and yields the lines that fall within the specified date range.
/// This could be made generic to support different message types in the future.
/// </summary>
public class MessageRetriever(ILogger<MessageRetriever> logger) : IMessageRetriever
{
    private const string InputFileName = "hearty_data.log"; // TODO: Make this configurable

    // Reads messages from the input file within the specified date range. 
    public IEnumerable<string> ReadByDateRange(DateTime start, DateTime end)
    {
        // TODO: some reasonable validation of the date ranges to be considered

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