using SmhiWeather.Application.Smhi;
using SmhiWeather.Application.Weather;

namespace SmhiWeather.UnitTests.Weather;

public class WeatherReadingsServiceTests
{
    private static readonly DateTimeOffset MeasuredAt = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetReadingsAsync_CombinesTemperatureWindGustAndWindSpeedDatasetsFromClient()
    {
        var temperature = new SmhiParameterDataset(
        [
            new SmhiStationSeries(
                "98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, "celsius",
                [new SmhiMeasurement(18.4, MeasuredAt, "G")]),
        ]);
        var windGust = new SmhiParameterDataset(
        [
            new SmhiStationSeries(
                "98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, "meter per sekund",
                [new SmhiMeasurement(9.7, MeasuredAt, "G")]),
        ]);
        var windSpeed = new SmhiParameterDataset(
        [
            new SmhiStationSeries(
                "98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, "meter per sekund",
                [new SmhiMeasurement(5.1, MeasuredAt, "G")]),
        ]);
        var client = new FakeSmhiClient(temperature, windGust, windSpeed);
        var service = new WeatherReadingsService(client);

        var result = await service.GetReadingsAsync("98230", WeatherPeriod.Hour, CancellationToken.None);

        var reading = Assert.Single(result);
        Assert.Equal(18.4, Assert.Single(reading.Temperature).Value);
        Assert.Equal(9.7, Assert.Single(reading.WindGust).Value);
        Assert.Equal(5.1, Assert.Single(reading.WindSpeed).Value);
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
