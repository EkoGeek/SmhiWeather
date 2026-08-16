using SmhiWeather.Application.Smhi;
using SmhiWeather.Application.Weather;

namespace SmhiWeather.UnitTests.Weather;

public class WeatherReadingCombinerTests
{
    private static readonly DateTimeOffset MeasuredAt = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Combine_StationPresentInBothDatasets_MergesTemperatureAndWindGust()
    {
        var temperature = new[]
        {
            new SmhiStationValue("98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, 18.4, "celsius", MeasuredAt, "G"),
        };
        var windGust = new[]
        {
            new SmhiStationValue("98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, 9.7, "meter per sekund", MeasuredAt, "G"),
        };

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        var reading = Assert.Single(result);
        Assert.Equal("98230", reading.StationId);
        Assert.Equal("Stockholm-Observatoriekullen A", reading.StationName);
        Assert.NotNull(reading.Temperature);
        Assert.Equal(18.4, reading.Temperature!.Value);
        Assert.NotNull(reading.WindGust);
        Assert.Equal(9.7, reading.WindGust!.Value);
    }

    [Fact]
    public void Combine_StationOnlyInTemperatureDataset_WindGustIsNull()
    {
        var temperature = new[]
        {
            new SmhiStationValue("1", "Only Temperature", null, null, 10.0, "celsius", MeasuredAt, "G"),
        };
        var windGust = Array.Empty<SmhiStationValue>();

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        var reading = Assert.Single(result);
        Assert.NotNull(reading.Temperature);
        Assert.Null(reading.WindGust);
    }

    [Fact]
    public void Combine_StationOnlyInWindGustDataset_TemperatureIsNull()
    {
        var temperature = Array.Empty<SmhiStationValue>();
        var windGust = new[]
        {
            new SmhiStationValue("2", "Only Wind", null, null, 5.4, "meter per sekund", MeasuredAt, "G"),
        };

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        var reading = Assert.Single(result);
        Assert.Null(reading.Temperature);
        Assert.NotNull(reading.WindGust);
    }

    [Fact]
    public void Combine_MultipleStationsAcrossBothDatasets_ReturnsUnionOrderedByStationName()
    {
        var temperature = new[]
        {
            new SmhiStationValue("1", "Charlie Station", null, null, 10.0, "celsius", MeasuredAt, "G"),
            new SmhiStationValue("2", "Alpha Station", null, null, 12.0, "celsius", MeasuredAt, "G"),
        };
        var windGust = new[]
        {
            new SmhiStationValue("2", "Alpha Station", null, null, 3.0, "meter per sekund", MeasuredAt, "G"),
            new SmhiStationValue("3", "Bravo Station", null, null, 4.0, "meter per sekund", MeasuredAt, "G"),
        };

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        Assert.Equal(["Alpha Station", "Bravo Station", "Charlie Station"], result.Select(r => r.StationName));
        Assert.Null(result.Single(r => r.StationName == "Charlie Station").WindGust);
        Assert.Null(result.Single(r => r.StationName == "Bravo Station").Temperature);
        Assert.NotNull(result.Single(r => r.StationName == "Alpha Station").Temperature);
        Assert.NotNull(result.Single(r => r.StationName == "Alpha Station").WindGust);
    }

    [Fact]
    public void Combine_NoDataInEitherDataset_ReturnsEmptyList()
    {
        var result = WeatherReadingCombiner.Combine([], []);

        Assert.Empty(result);
    }
}
