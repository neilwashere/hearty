using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;

/// <summary>
/// This is a test websocket client to scope out the upstream TWWWSS WebSocket server.
/// </summary>
/// <param name="logger"></param>
/// <param name="messageValidator"></param>
public class WebSocketClient(ILogger<WebSocketClient> logger, IMessageValidator messageValidator)
{
    private readonly ILogger<WebSocketClient> _logger = logger;
    private readonly IMessageValidator _messageValidator = messageValidator;
    private static readonly string UPSTREAM_URL = "ws://aide-twwwss-be02d4b95847.herokuapp.com/ws";

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WebSocket Client starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var client = new ClientWebSocket();
                // client.Options.KeepAliveInterval = _keepAliveInterval;

                _logger.LogInformation("Attempting to connect to WebSocket server at {Url}...", UPSTREAM_URL);

                // Use a connection-specific token that can be cancelled on disconnect
                using var connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, connectCts.Token);

                await client.ConnectAsync(new Uri(UPSTREAM_URL), linkedCts.Token);
                _logger.LogInformation("✅ Successfully connected to WebSocket server.");

                // Once connected, listen for messages until disconnected or app shuts down
                await ReadMessagesAsync(client, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // This is expected on graceful shutdown (from stoppingToken) 
                // or connection timeout (from connectCts).
                // If it's a shutdown, the outer loop will terminate.
                _logger.LogWarning("Connection attempt timed out or was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Connection to WebSocket failed or was lost unexpectedly.");
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break; // Exit immediately if shutdown was requested
            }

            _logger.LogInformation("Will retry connection in {RetryDelay}...", 10000);
            await Task.Delay(10000, stoppingToken);
        }

        _logger.LogInformation("Robust WebSocket Client stopped.");
    }

    private async Task ReadMessagesAsync(ClientWebSocket client, CancellationToken cancellationToken)
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);

        while (client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await client.ReceiveAsync(buffer, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                _logger.LogWarning("🔌 Server initiated close. Status: {Status}, Description: {Description}",
                    result.CloseStatus, result.CloseStatusDescription);
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client acknowledging close", CancellationToken.None);
                break; // Exit the read loop to trigger a reconnection attempt by the outer loop
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer.AsSpan(0, result.Count));

                // Validate the message using the injected validator
                if (_messageValidator.IsValid(message))
                {
                    _logger.LogInformation("✅ Valid message received: {Message}", message);
                }
                else
                {
                    _logger.LogWarning("❌ Invalid message received: {Message}", message);
                }   
            }
            else
            {
                _logger.LogWarning("Received unsupported message type: {MessageType}", result.MessageType);
            }
        }
    }
}