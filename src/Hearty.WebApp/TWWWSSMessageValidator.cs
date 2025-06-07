using System.Text.Json;
using System.Text.Json.Serialization;

public class TWWWSSMessage {

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class TWWWSSMessageValidator : IMessageValidator
{
    private readonly ILogger<TWWWSSMessageValidator> _logger;

    public TWWWSSMessageValidator(ILogger<TWWWSSMessageValidator> logger)
    {
        _logger = logger;
    }

    public bool IsValid(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Received null or empty message, which is not valid.");
            return false;
        }

        try
        {
            var heartyMessage = JsonSerializer.Deserialize<TWWWSSMessage>(message);
            if (heartyMessage == null)
            {
                _logger.LogWarning("Deserialized message is null");
                return false;
            }

            if (heartyMessage.Timestamp <= 0)
            {
                _logger.LogWarning("Timestamp is not set correctly: {Timestamp}", heartyMessage.Timestamp);
                return false;
            }

            if (heartyMessage.Value < 0)
            {
                _logger.LogWarning("Value is not set correctly: {Value}", heartyMessage.Value);
                return false;
            }

            return true;
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize message: {Message}", message);
            return false;
        }
    }
}