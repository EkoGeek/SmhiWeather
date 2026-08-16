using SmhiWeather.Application.Smhi;
using SmhiWeather.Application.Weather;

namespace SmhiWeather.UnitTests.Weather;

public class WeatherReadingCombinerTests
{
    private static readonly DateTimeOffset MeasuredAt = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly SmhiStationSeries[] NoStations = [];

    private static SmhiStationSeries Series(string stationId, string name, string unit, params double[] values) =>
        new(stationId, name, 59.34, 18.05, unit, values.Select((v, i) => new SmhiMeasurement(v, MeasuredAt.AddHours(i), "G")).ToList());

    [Fact]
    public void Combine_StationPresentInAllThreeDatasets_MergesTemperatureWindGustAndWindSpeed()
    {
        var temperature = new[] { Series("98230", "Stockholm-Observatoriekullen A", "celsius", 18.4) };
        var windGust = new[] { Series("98230", "Stockholm-Observatoriekullen A", "meter per sekund", 9.7) };
        var windSpeed = new[] { Series("98230", "Stockholm-Observatoriekullen A", "meter per sekund", 5.1) };

        var result = WeatherReadingCombiner.Combine(temperature, windGust, windSpeed);

        var reading = Assert.Single(result);
        Assert.Equal("98230", reading.StationId);
        Assert.Equal("Stockholm-Observatoriekullen A", reading.StationName);
        Assert.Equal(18.4, Assert.Single(reading.Temperature).Value);
        Assert.Equal(9.7, Assert.Single(reading.WindGust).Value);
        Assert.Equal(5.1, Assert.Single(reading.WindSpeed).Value);
    }

    [Fact]
    public void Combine_StationOnlyInTemperatureDataset_WindGustAndWindSpeedAreEmpty()
    {
        var temperature = new[] { Series("1", "Only Temperature", "celsius", 10.0) };

        var result = WeatherReadingCombiner.Combine(temperature, NoStations, NoStations);

        var reading = Assert.Single(result);
        Assert.NotEmpty(reading.Temperature);
        Assert.Empty(reading.WindGust);
        Assert.Empty(reading.WindSpeed);
    }

    [Fact]
    public void Combine_StationOnlyInWindSpeedDataset_TemperatureAndWindGustAreEmpty()
    {
        var windSpeed = new[] { Series("4", "Only Wind Speed", "meter per sekund", 3.2) };

        var result = WeatherReadingCombiner.Combine(NoStations, NoStations, windSpeed);

        var reading = Assert.Single(result);
        Assert.Empty(reading.Temperature);
        Assert.Empty(reading.WindGust);
        Assert.NotEmpty(reading.WindSpeed);
    }

    [Fact]
    public void Combine_MultipleStationsAcrossThreeDatasets_ReturnsUnionOrderedByStationName()
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
        var windSpeed = new[] { Series("3", "Bravo Station", "meter per sekund", 2.5) };

        var result = WeatherReadingCombiner.Combine(temperature, windGust, windSpeed);

        Assert.Equal(["Alpha Station", "Bravo Station", "Charlie Station"], result.Select(r => r.StationName));

        var charlie = result.Single(r => r.StationName == "Charlie Station");
        Assert.NotEmpty(charlie.Temperature);
        Assert.Empty(charlie.WindGust);
        Assert.Empty(charlie.WindSpeed);

        var bravo = result.Single(r => r.StationName == "Bravo Station");
        Assert.Empty(bravo.Temperature);
        Assert.NotEmpty(bravo.WindGust);
        Assert.NotEmpty(bravo.WindSpeed);

        var alpha = result.Single(r => r.StationName == "Alpha Station");
        Assert.NotEmpty(alpha.Temperature);
        Assert.NotEmpty(alpha.WindGust);
        Assert.Empty(alpha.WindSpeed);
    }

    [Fact]
    public void Combine_MultiValueDaySeries_KeepsEveryMeasurementForAllThreeParameters()
    {
        // SMHI's period=day feed returns one value per hour rather than a single latest value.
        var temperature = new[] { Series("98230", "Stockholm-Observatoriekullen A", "celsius", 26.5, 25.1, 16.8) };
        var windGust = new[] { Series("98230", "Stockholm-Observatoriekullen A", "meter per sekund", 4.0, 5.5) };
        var windSpeed = new[] { Series("98230", "Stockholm-Observatoriekullen A", "meter per sekund", 2.1, 3.4) };

        var result = WeatherReadingCombiner.Combine(temperature, windGust, windSpeed);

        var reading = Assert.Single(result);
        Assert.Equal([26.5, 25.1, 16.8], reading.Temperature.Select(r => r.Value));
        Assert.Equal([4.0, 5.5], reading.WindGust.Select(r => r.Value));
        Assert.Equal([2.1, 3.4], reading.WindSpeed.Select(r => r.Value));
    }

    [Fact]
    public void Combine_NoDataInAnyDataset_ReturnsEmptyList()
    {
        var result = WeatherReadingCombiner.Combine(NoStations, NoStations, NoStations);

        Assert.Empty(result);
    }
}
