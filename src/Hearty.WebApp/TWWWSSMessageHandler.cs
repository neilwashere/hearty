using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

public class TWWWSSMessage {

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class TWWWSSMessageHandler(
    ILogger<TWWWSSMessageHandler> logger
    ,ChannelWriter<TWWWSSMessage> channelWriter
    ): IMessageHandler {

    public async Task HandleMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            logger.LogWarning("Received null or empty message, which is not valid.");
            return;
        }

        try
        {
            var heartyMessage = JsonSerializer.Deserialize<TWWWSSMessage>(message);
            if (heartyMessage == null)
            {
                logger.LogWarning("Deserialized message is null");
                return;
            }

            if (heartyMessage.Timestamp <= 0)
            {
                logger.LogWarning("⏱️ Timestamp is not set correctly: {Timestamp}", heartyMessage.Timestamp);
                return;
            }

            if (heartyMessage.Value < 0)
            {
                logger.LogWarning("#️ Value is not set correctly: {Value}", heartyMessage.Value);
                return;
            }

            logger.LogInformation("✅ Valid message: {Message}", message);

            await channelWriter.WriteAsync(heartyMessage);

        }
        catch (JsonException)
        {
            logger.LogWarning("💥 Invalid JSON: {Message}", message);
            return;
        }
    }
}