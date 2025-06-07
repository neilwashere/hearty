using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SignalRClient;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices((hostContext, services) =>
        {
            services.AddTransient<IMessageValidator, HeartyMessageValidator>();
            services.AddSingleton<WebSocketClient>();
            services.AddHostedService<SignalRClientService>();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole(options => options.TimestampFormat = "HH:mm:ss ");
        });

        // var host = builder.Build();

        // Handle graceful shutdown on Ctrl+C
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        // var client = host.Services.GetRequiredService<WebSocketClient>();

        // // Start the WebSocket client
        // await client.ExecuteAsync(CancellationToken.None);
        await builder.RunConsoleAsync(cts.Token);
    }
}
