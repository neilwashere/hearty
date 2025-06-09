using System.Threading.Channels;

/// <summary>
/// This service persists messages from a channel to a file in a background task.
/// It reads messages from the channel and appends them to a log file in JSON format.
/// This could be made generic to support different message types in the future.
/// </summary>
public class MessagePersistor(
    ILogger<MessagePersistor> logger,
    ChannelReader<TWWWSSMessage> channelReader,
    IConfiguration configuration
) : BackgroundService
{
    private readonly string outputFileName =
        configuration.GetValue<string>("Hearty:PersistenceFilePath")!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("📂 File Processor Service is starting.");

        try
        {
            // Create the file if it does not exist
            await File.WriteAllTextAsync(outputFileName, string.Empty, stoppingToken);
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "💥 Failed to create file {FileName}.", outputFileName);
            return; // Exit if we can't create the file
        }

        try
        {
            await foreach (var data in channelReader.ReadAllAsync(stoppingToken))
            {
                var lineToWrite = $"{{\"timestamp\": {data.Timestamp}, \"value\": {data.Value}}}";

                try
                {
                    // Append the single line to the file immediately.
                    logger.LogInformation("📥 Writing line to file: {Line}", lineToWrite);

                    await File.AppendAllTextAsync(outputFileName, lineToWrite + Environment.NewLine, stoppingToken);
                }
                catch (IOException ex)
                {
                    logger.LogError(ex, "💥 Failed to write to file {FileName}.", outputFileName);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("File processing was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "💥 An unexpected error occurred in the file processor service.");
        }
        finally
        {
            logger.LogInformation("📁 File Processor Service has finished.");
        }
    }
}
