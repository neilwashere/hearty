
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hearty.WebApp.Pages
{
    public class HistoricalModel(
        ILogger<HistoricalModel> logger,
        ITimeSeriesMessageRetriever<TWWWSSMessage> messageRetriever
        ) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int TimeValue { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TimeUnit { get; set; } = "minutes";

        public string ChartDataJson { get; private set; } = string.Empty;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public void OnGet()
        {
            if (TimeValue > 0)
            {
                logger.LogInformation("Fetching historical data for {TimeValue} {TimeUnit}", TimeValue, TimeUnit);

                var endDate = DateTime.UtcNow;
                var startDate = TimeUnit switch
                {
                    "seconds" => endDate.AddSeconds(-TimeValue),
                    "minutes" => endDate.AddMinutes(-TimeValue),
                    "hours" => endDate.AddHours(-TimeValue),
                    "days" => endDate.AddDays(-TimeValue),
                    _ => throw new ArgumentException("Invalid time unit")
                };

                var messages = messageRetriever.ReadByDateRange(startDate, endDate);
                if (messages.Any())
                {
                    logger.LogInformation("Historical data fetched successfully for {TimeValue} {TimeUnit}", TimeValue, TimeUnit);
                    ChartDataJson = JsonSerializer.Serialize(messages.ToArray(), JsonOptions);
                }
                else
                {
                    logger.LogWarning("No messages found for the specified time range: {StartDate} to {EndDate}", startDate, endDate);
                }


            }
            else
            {
                // We don't want the initial page load to pull data so it should
                // be zero by default. However, we want the page form to have
                // a sane default value on first render.
                TimeValue = 10; // Reset to default value
            }

        }
    }
}
