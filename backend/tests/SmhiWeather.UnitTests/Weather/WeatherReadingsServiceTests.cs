using SmhiWeather.Application.Smhi;
using SmhiWeather.Application.Weather;

namespace SmhiWeather.UnitTests.Weather;

public class WeatherReadingsServiceTests
{
    private static readonly DateTimeOffset MeasuredAt = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetReadingsAsync_CombinesTemperatureAndWindGustDatasetsFromClient()
    {
        var temperature = new SmhiParameterDataset(
        [
            new SmhiStationValue("98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, 18.4, "celsius", MeasuredAt, "G"),
        ]);
        var windGust = new SmhiParameterDataset(
        [
            new SmhiStationValue("98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, 9.7, "meter per sekund", MeasuredAt, "G"),
        ]);
        var client = new FakeSmhiClient(temperature, windGust);
        var service = new WeatherReadingsService(client);

        var result = await service.GetReadingsAsync("98230", WeatherPeriod.Hour, CancellationToken.None);

        var reading = Assert.Single(result);
        Assert.Equal(18.4, reading.Temperature!.Value);
        Assert.Equal(9.7, reading.WindGust!.Value);
        Assert.Equal("98230", client.RequestedStationId);
        Assert.Equal(WeatherPeriod.Hour, client.RequestedPeriod);
    }

    [Fact]
    public async Task GetReadingsAsync_NoStationIdRequestsAllStations()
    {
        var client = new FakeSmhiClient(new SmhiParameterDataset([]), new SmhiParameterDataset([]));
        var service = new WeatherReadingsService(client);

        var result = await service.GetReadingsAsync(null, WeatherPeriod.Day, CancellationToken.None);

        Assert.Empty(result);
        Assert.Null(client.RequestedStationId);
        Assert.Equal(WeatherPeriod.Day, client.RequestedPeriod);
    }
}
