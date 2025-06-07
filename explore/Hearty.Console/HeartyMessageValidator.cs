
using Microsoft.Extensions.Logging;

public interface IMessageValidator
{
    bool IsValid(string message);
}

// This class represents the structure of a valid message that we expect to receive.
public class HeartyMessage
{
    [System.Text.Json.Serialization.JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("value")]
    public int Value { get; set; }
}

// This class checks if the passed message is indeed a valid HeartyMessage.
// TODO: This is a very basic implementation and could be improved for clarity and robustness. 
public class HeartyMessageValidator : IMessageValidator
{
    private ILogger<HeartyMessageValidator> _logger;

    public HeartyMessageValidator(ILogger<HeartyMessageValidator> logger)
    {
        _logger = logger;
    }

    public bool IsValid(string message)
    {

        // Check if the message is null

        if (message == null)
        {
            _logger.LogWarning("Received null message, which is not valid.");
            return false;
        }

        // Attempt to deserialize the message to a HeartyMessage object
        HeartyMessage? heartyMessage;
        try
        {
            // Deserialize the message using System.Text.Json
            heartyMessage = System.Text.Json.JsonSerializer.Deserialize<HeartyMessage>(message);
        }
        catch (System.Text.Json.JsonException)
        {
            _logger.LogWarning("Failed to deserialize message: {Message}", message);
            return false;
        }

        // Check if heartyMessage is not null and Timestamp is not default (i.e., it has been set)
        if (heartyMessage == null)
        {
            _logger.LogWarning("Deserialized message is null");
            return false;
        }

        if (heartyMessage.Timestamp == default || heartyMessage.Timestamp <= 0)
        {
            _logger.LogWarning("Timestamp is not set correctly : {timestamp}", heartyMessage.Timestamp);
            return false;
        }

        // Check if Value is set and is a valid integer
        if (heartyMessage.Value < 0)
        {
            _logger.LogWarning("Value is not set correctly : {value}", heartyMessage.Value);
            return false;
        }

        // If all checks pass, the message is valid
        return true;
    }
}