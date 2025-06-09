using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;

public class TWWWSSIngestorIntegrationTest
{
    /// <summary>
    /// This test verifies that the TWWWSSIngestor can handle a valid message and write it to the message handler.
    /// It calls the remote TWWWSS debug stream.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Ingestor_Handles_Valid_Message_And_Writes_To_Handler()
    {
        var loggerMock = new Mock<ILogger<TWWWSSIngestor>>();
        var messageHandlerMock = new Mock<IMessageHandler>();
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").Build();


        // Setup the handler to expect a valid message
        messageHandlerMock
            .Setup(m => m.HandleMessageAsync(It.IsAny<string>()))
            .Returns<string>(async msg =>
            {
                // This will kill our test if the message is not valid
                var parsed = System.Text.Json.JsonSerializer.Deserialize<TWWWSSMessage>(msg);

                await Task.CompletedTask;
            });

        // Create the ingestor
        var ingestor = new TWWWSSIngestor(
            loggerMock.Object,
            messageHandlerMock.Object,
            config
        );

        await ingestor.StartAsync(CancellationToken.None);
        // Wait a couple of seconds to allow the ingestor to connect and start processing messages
        await Task.Delay(2000);

        // Assert that the handler was called with a valid message
        messageHandlerMock.Verify(m => m.HandleMessageAsync(It.IsAny<string>()), Times.AtLeastOnce);

        // Stop the ingestor
        await ingestor.StopAsync(CancellationToken.None);

    }
}
