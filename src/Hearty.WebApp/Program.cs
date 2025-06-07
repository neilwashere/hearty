var builder = WebApplication.CreateBuilder(args);

// Setup logging. This is simple console logging but we'd want a structured logger in production
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.TimestampFormat = "HH:mm:ss ");
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Add our message validator
builder.Services.AddSingleton<IMessageHandler, TWWWSSMessageHandler>();

// Add a background service to the DI container
builder.Services.AddHostedService<TWWWSSIngestor>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
