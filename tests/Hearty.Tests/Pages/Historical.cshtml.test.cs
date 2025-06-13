using Hearty.WebApp.Pages;
using Microsoft.Extensions.Logging;
using Moq;

public class HistoricalModelTests
{
    [Fact]
    public void OnGet_WithValidTimeValueAndMessages_SetsChartDataJson()
    {
        var loggerMock = new Mock<ILogger<HistoricalModel>>();
        var retrieverMock = new Mock<ITimeSeriesMessageRetriever<TWWWSSMessage>>();

        var testMessages = new List<TWWWSSMessage>
        {
            new() { Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Value = 42 }
        };

        retrieverMock
            .Setup(r => r.ReadByDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(testMessages);

        var model = new HistoricalModel(loggerMock.Object, retrieverMock.Object)
        {
            TimeValue = 5,
            TimeUnit = "minutes"
        };

        model.OnGet();

        Assert.False(string.IsNullOrWhiteSpace(model.ChartDataJson));
        Assert.Contains("42", model.ChartDataJson);
    }

    [Fact]
    public void OnGet_WithNoMessages_SetsEmptyChartDataJson()
    {
        var loggerMock = new Mock<ILogger<HistoricalModel>>();
        var retrieverMock = new Mock<ITimeSeriesMessageRetriever<TWWWSSMessage>>();

        retrieverMock
            .Setup(r => r.ReadByDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns([]);

        var model = new HistoricalModel(loggerMock.Object, retrieverMock.Object)
        {
            TimeValue = 5,
            TimeUnit = "minutes"
        };

        model.OnGet();

        Assert.True(string.IsNullOrWhiteSpace(model.ChartDataJson) || model.ChartDataJson == "[]");
    }

    [Fact]
    public void OnGet_WithZeroTimeValue_ResetsToDefault()
    {
        var loggerMock = new Mock<ILogger<HistoricalModel>>();
        var retrieverMock = new Mock<ITimeSeriesMessageRetriever<TWWWSSMessage>>();

        var model = new HistoricalModel(loggerMock.Object, retrieverMock.Object)
        {
            TimeValue = 0,
            TimeUnit = "minutes"
        };

        model.OnGet();

        Assert.Equal(10, model.TimeValue);
    }
}
