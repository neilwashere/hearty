using System.Text.Json;

public interface ITimeSeriesMessageRetriever<T>
{
    /// <summary>
    /// Reads messages from the persistence store within the specified date range.
    /// </summary>
    /// <param name="start">The start date and time of the range.</param>
    /// <param name="end">The end date and time of the range.</param>
    /// <returns>An enumerable collection of messages as strings.</returns>
    IEnumerable<T> ReadByDateRange(DateTime start, DateTime end);
}

/// <summary>
/// This class retrieves messages from a file within a specified date range.
/// It reads the file line by line, deserializes each line into a TWWWSSMessage object,
/// and yields the lines that fall within the specified date range.
/// TODO - make generic to support different message types.
/// </summary>
public class MessageRetriever : ITimeSeriesMessageRetriever<TWWWSSMessage>
{
    private readonly ILogger<MessageRetriever> logger;
    private readonly string inputFileName;

    public MessageRetriever(ILogger<MessageRetriever> logger, IConfiguration configuration)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        inputFileName = configuration.GetValue<string>("Hearty:PersistenceFilePath")!;

        // Create the file if it does not exist
        // TODO: This is a bit of a hack to deal with test isolation issues and, of course, because
        // we are using a file for persistence! (do not try this at home, kids!)
        if (!File.Exists(inputFileName))
        {
            try
            {
                File.Create(inputFileName).Dispose(); // Ensure the file is created
            }
            catch (IOException ex)
            {
                logger.LogError(ex, "💥 Failed to create input file {InputFileName}.", inputFileName);
                throw; // Rethrow to ensure the application can handle this failure
            }
        }
    }

    // Reads messages from the input file within the specified date range.
    public IEnumerable<TWWWSSMessage> ReadByDateRange(DateTime start, DateTime end)
    {
        // TODO: some reasonable validation of the date ranges to be considered

        // Convert the start and end DateTime to Unix epoch milliseconds.
        long startTimestamp = new DateTimeOffset(start).ToUnixTimeMilliseconds();
        long endTimestamp = new DateTimeOffset(end).ToUnixTimeMilliseconds();

        logger.LogInformation("⌛ Retrieving messages from {Start} to {End} (timestamps: {StartTimestamp} to {EndTimestamp})",
            start, end, startTimestamp, endTimestamp);

        // Use an iterator to efficiently yield matching lines.
        // Allow the caller to figure out how to present the data (eg. wrapping into an array).
        foreach (var line in File.ReadLines(inputFileName))
        {
            var record = default(TWWWSSMessage);
            bool isValidRecord = false;
            try
            {
                record = JsonSerializer.Deserialize<TWWWSSMessage>(line);

                // Check if the timestamp is within the specified range.
                if (record != null && record.Timestamp >= startTimestamp && record.Timestamp <= endTimestamp)
                    isValidRecord = true;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "💥 Invalid JSON line: {Line} from persistence!", line);
            }

            if (isValidRecord)
                yield return record!;
        }
    }
}
