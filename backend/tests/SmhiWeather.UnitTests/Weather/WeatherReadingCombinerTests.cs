using SmhiWeather.Application.Smhi;
using SmhiWeather.Application.Weather;

namespace SmhiWeather.UnitTests.Weather;

public class WeatherReadingCombinerTests
{
    private static readonly DateTimeOffset MeasuredAt = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    private static SmhiStationSeries Series(string stationId, string name, string unit, params double[] values) =>
        new(stationId, name, 59.34, 18.05, unit, values.Select((v, i) => new SmhiMeasurement(v, MeasuredAt.AddHours(i), "G")).ToList());

    [Fact]
    public void Combine_StationPresentInBothDatasets_MergesTemperatureAndWindGust()
    {
        var temperature = new[] { Series("98230", "Stockholm-Observatoriekullen A", "celsius", 18.4) };
        var windGust = new[] { Series("98230", "Stockholm-Observatoriekullen A", "meter per sekund", 9.7) };

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        var reading = Assert.Single(result);
        Assert.Equal("98230", reading.StationId);
        Assert.Equal("Stockholm-Observatoriekullen A", reading.StationName);
        Assert.Equal(18.4, Assert.Single(reading.Temperature).Value);
        Assert.Equal(9.7, Assert.Single(reading.WindGust).Value);
    }

    [Fact]
    public void Combine_StationOnlyInTemperatureDataset_WindGustIsEmpty()
    {
        var temperature = new[] { Series("1", "Only Temperature", "celsius", 10.0) };
        var windGust = Array.Empty<SmhiStationSeries>();

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        var reading = Assert.Single(result);
        Assert.NotEmpty(reading.Temperature);
        Assert.Empty(reading.WindGust);
    }

    [Fact]
    public void Combine_StationOnlyInWindGustDataset_TemperatureIsEmpty()
    {
        var temperature = Array.Empty<SmhiStationSeries>();
        var windGust = new[] { Series("2", "Only Wind", "meter per sekund", 5.4) };

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        var reading = Assert.Single(result);
        Assert.Empty(reading.Temperature);
        Assert.NotEmpty(reading.WindGust);
    }

    [Fact]
    public void Combine_MultipleStationsAcrossBothDatasets_ReturnsUnionOrderedByStationName()
    {
        var temperature = new[]
        {
            Series("1", "Charlie Station", "celsius", 10.0),
            Series("2", "Alpha Station", "celsius", 12.0),
        };
        var windGust = new[]
        {
            Series("2", "Alpha Station", "meter per sekund", 3.0),
            Series("3", "Bravo Station", "meter per sekund", 4.0),
        };

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        Assert.Equal(["Alpha Station", "Bravo Station", "Charlie Station"], result.Select(r => r.StationName));
        Assert.Empty(result.Single(r => r.StationName == "Charlie Station").WindGust);
        Assert.Empty(result.Single(r => r.StationName == "Bravo Station").Temperature);
        Assert.NotEmpty(result.Single(r => r.StationName == "Alpha Station").Temperature);
        Assert.NotEmpty(result.Single(r => r.StationName == "Alpha Station").WindGust);
    }

    [Fact]
    public void Combine_MultiValueDaySeries_KeepsEveryMeasurementForBothParameters()
    {
        // SMHI's period=day feed returns one value per hour rather than a single latest value.
        var temperature = new[] { Series("98230", "Stockholm-Observatoriekullen A", "celsius", 26.5, 25.1, 16.8) };
        var windGust = new[] { Series("98230", "Stockholm-Observatoriekullen A", "meter per sekund", 4.0, 5.5) };

        var result = WeatherReadingCombiner.Combine(temperature, windGust);

        var reading = Assert.Single(result);
        Assert.Equal([26.5, 25.1, 16.8], reading.Temperature.Select(r => r.Value));
        Assert.Equal([4.0, 5.5], reading.WindGust.Select(r => r.Value));
    }

    [Fact]
    public void Combine_NoDataInEitherDataset_ReturnsEmptyList()
    {
        var result = WeatherReadingCombiner.Combine([], []);

        Assert.Empty(result);
    }
}
