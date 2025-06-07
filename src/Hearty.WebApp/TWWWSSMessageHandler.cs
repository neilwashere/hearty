using System.Text.Json;
using System.Text.Json.Serialization;

public class TWWWSSMessage {

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class TWWWSSMessageHandler(
    ILogger<TWWWSSMessageHandler> logger): IMessageHandler {

    public Task HandleMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            logger.LogWarning("Received null or empty message, which is not valid.");
            return Task.CompletedTask;
        }

        try
        {
            var heartyMessage = JsonSerializer.Deserialize<TWWWSSMessage>(message);
            if (heartyMessage == null)
            {
                logger.LogWarning("Deserialized message is null");
                return Task.CompletedTask;
            }

            if (heartyMessage.Timestamp <= 0)
            {
                logger.LogWarning("⏱️ Timestamp is not set correctly: {Timestamp}", heartyMessage.Timestamp);
                return Task.CompletedTask;
            }

            if (heartyMessage.Value < 0)
            {
                logger.LogWarning("#️ Value is not set correctly: {Value}", heartyMessage.Value);
                return Task.CompletedTask;
            }

        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogWarning("💥 Invalid JSON: {Message}", message);
            return Task.CompletedTask;
        }

        logger.LogInformation("✅ Valid message: {Message}", message);

        return Task.CompletedTask;
    }
}