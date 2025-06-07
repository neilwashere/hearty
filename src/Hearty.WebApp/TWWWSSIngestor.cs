// Background service to ingest TWWWSS data from a websocket

using System.Net.WebSockets;
using System.Text;

public interface IMessageValidator
{
    bool IsValid(string message);
}

public class TWWWSSIngestor(
    ILogger<TWWWSSIngestor> logger,
    IMessageValidator messageValidator,
    IConfiguration configuration) : BackgroundService
{
    private readonly string upstreamUrl = configuration.GetValue<string>("TWWWSS:UpstreamUrl") ??
        throw new ArgumentNullException("TWWWSS:UpstreamUrl configuration is missing");

    private readonly int reconnectDelayMillis = 5000; // Default reconnect delay in seconds - to be configurable

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("WebSocket Client starting.");

        // Create a linked cancellation token source that combines the stopping token
        // This will all for a graceful reconnection attempt.
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var client = new ClientWebSocket();

                logger.LogInformation("Attempting to connect to WebSocket server at {Url}...", upstreamUrl);

                await client.ConnectAsync(new Uri(upstreamUrl), linkedCts.Token);
                logger.LogInformation("🔌 Successfully connected to WebSocket server.");

                // listen for messages until disconnected or app shuts down
                await ReadMessagesAsync(client, linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                // This is expected on graceful shutdown (from stoppingToken) 
                // or connection timeout 
                logger.LogWarning("🔌 Connection attempt timed out or was cancelled.");
                break; // Exit the loop to retry connection
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Connection to WebSocket failed or was lost unexpectedly.");
                if (!linkedCts.IsCancellationRequested)
                {
                    // Stop ongoing read requests before attempting to reconnect
                    await linkedCts.CancelAsync();
                }
            }

            // If we reach here, it means we either disconnected or an error occurred
            // We will wait for a little while before reconnnecting if the app isn't shutting down
            // TODO In production we would want to use some exponential backoff strategy, bulkhead or other failure handling strategies.
            if (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("🔌 Will retry connection in {reconnectDelayMillis}...", reconnectDelayMillis);

                await Task.Delay(reconnectDelayMillis, stoppingToken);
                linkedCts.Dispose();
                linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            }


        }

        logger.LogInformation("🔌 TWWWSSIngestor stopped.");
    }

    private async Task ReadMessagesAsync(ClientWebSocket client, CancellationToken cancellationToken)
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);

        while (client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await client.ReceiveAsync(buffer, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                logger.LogWarning("🔌 Server initiated close. Status: {Status}, Description: {Description}",
                    result.CloseStatus, result.CloseStatusDescription);
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "🔌 Client acknowledging close", CancellationToken.None);
                break; // Exit the read loop to trigger a reconnection attempt
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer.AsSpan(0, result.Count));

                // Validate the message using the injected validator
                if (messageValidator.IsValid(message))
                {
                    logger.LogInformation("✔️ Valid message received: {Message}", message);
                }
                else
                {
                    logger.LogWarning("❌ Invalid message received: {Message}", message);
                }
            }
            else
            {
                logger.LogWarning("⛔ Received unsupported message type: {MessageType}", result.MessageType);
            }
        }
    }
}
