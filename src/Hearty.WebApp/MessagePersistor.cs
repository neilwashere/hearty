using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

/// <summary>
/// This is the core background service that fulfills the request.
/// It continuously reads from the channel and appends the data to a file
/// as soon as it is received.
/// </summary>
public class MessagePersistor(
    ILogger<MessagePersistor> logger,
    ChannelReader<TWWWSSMessage> channelReader
) : BackgroundService
{
    private const string OutputFileName = "hearty_data.log";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("📂 File Processor Service is starting.");

        try
        {
            await foreach (var data in channelReader.ReadAllAsync(stoppingToken))
            {
                var lineToWrite = $"{{\"timestamp\": {data.Timestamp}, \"value\": {data.Value}}}";

                try
                {
                    // Append the single line to the file immediately.
                    await File.AppendAllTextAsync(OutputFileName, lineToWrite + Environment.NewLine, stoppingToken);
                }
                catch (IOException ex)
                {
                    logger.LogError(ex, "💥 Failed to write to file {FileName}.", OutputFileName);
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