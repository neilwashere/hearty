using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json.Serialization;

public class TWWWSSMessage {

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

namespace SignalRClient
{
    public class SignalRClientService(ILogger<SignalRClientService> logger) : IHostedService, IAsyncDisposable
    {
        
        private HubConnection? connection;
        private static string HubUrl = "http://localhost:5030/stream"; 

        // The service constructor gets dependencies injected by the DI container.

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Get the Hub URL from our configuration (appsettings.json).

            // Create the HubConnection.
            connection = new HubConnectionBuilder()
                .WithUrl(HubUrl)
                .WithAutomaticReconnect()
                .Build();

            // --- Register a handler for the "ReceiveMessage" method ---
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                logger.LogInformation("[{Timestamp}] {User}: {Message}", DateTime.Now.ToString("HH:mm:ss"), user, message);
            });

            // --- Handle connection events ---
            connection.Reconnecting += error =>
            {
                logger.LogWarning("Connection lost. Reconnecting... Error: {ErrorMessage}", error?.Message);
                return Task.CompletedTask;
            };

            connection.Reconnected += connectionId =>
            {
                logger.LogInformation("Connection re-established with ID: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

             connection.Closed += error =>
            {
                logger.LogInformation("Connection closed. Error: {ErrorMessage}", error?.Message);
                return Task.CompletedTask;
            };

            // --- Start the Connection ---
            try
            {
                await connection.StartAsync(cancellationToken);
                logger.LogInformation("Connection started successfully. Listening for messages.");

                await foreach (var message in connection.StreamAsync<TWWWSSMessage>("StreamData", cancellationToken))
                {
                    // Process each message received from the server
                    logger.LogInformation("Received message: {timestamp} - {value}", message.Timestamp, message.Value);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting SignalR connection.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (connection != null)
            {
                logger.LogInformation("Closing SignalR connection.");
                await connection.StopAsync(cancellationToken);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }
        }
    }
}