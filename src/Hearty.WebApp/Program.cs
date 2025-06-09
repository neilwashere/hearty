using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Setup logging. This is simple console logging but we'd want a structured logger in production
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.TimestampFormat = "HH:mm:ss ");
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Simple pubsub implementation using in-memory channels
builder.Services.AddSingleton(Channel.CreateUnbounded<TWWWSSMessage>());
builder.Services.AddSingleton(sp =>
{
    var channel = sp.GetRequiredService<Channel<TWWWSSMessage>>();
    return channel.Writer;
});
builder.Services.AddSingleton(sp =>
{
    var channel = sp.GetRequiredService<Channel<TWWWSSMessage>>();
    return channel.Reader;
});

// Add our message validator
builder.Services.AddSingleton<IMessageHandler, TWWWSSMessageHandler>();

// Add TWWWSS stream consumer
builder.Services.AddHostedService<TWWWSSIngestor>();
// Add message persistor
builder.Services.AddHostedService<MessagePersistor>();
// Add our message retriever
builder.Services.AddScoped<IMessageRetriever, MessageRetriever>();

// Add SignalR for data streaming
builder.Services.AddSignalR();


var app = builder.Build();

// Endpoint to retrieve messages based on a date range
app.MapGet("/messages", (IMessageRetriever retriever, DateTime start, DateTime end) =>
{
    var messages = retriever.ReadByDateRange(start, end);
    return Results.Ok(messages.ToList());
});

// Streaming endpoint for real-time data
app.MapHub<MessageStreamer>("/stream");

// Serve static and default files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

public partial class Program { }
