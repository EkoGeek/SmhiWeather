using SmhiWeather.Application.Smhi;
using SmhiWeather.Domain;

namespace SmhiWeather.Application.Weather;

/// <summary>
/// Merges independently-fetched Lufttemperatur and Byvind datasets into one reading per station.
/// A station that only appears in one dataset is still included, with the other parameter empty.
/// </summary>
public static class WeatherReadingCombiner
{
    public static IReadOnlyList<WeatherStationReading> Combine(
        IReadOnlyList<SmhiStationSeries> temperature,
        IReadOnlyList<SmhiStationSeries> windGust)
    {
        var temperatureByStation = temperature.ToDictionary(series => series.StationId);
        var windGustByStation = windGust.ToDictionary(series => series.StationId);
        var stationIds = temperatureByStation.Keys.Union(windGustByStation.Keys);

        return stationIds
            .Select(stationId =>
            {
                temperatureByStation.TryGetValue(stationId, out var temperatureSeries);
                windGustByStation.TryGetValue(stationId, out var windGustSeries);
                return ToStationReading(stationId, temperatureSeries, windGustSeries);
            })
            .OrderBy(reading => reading.StationName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static WeatherStationReading ToStationReading(
        string stationId,
        SmhiStationSeries? temperature,
        SmhiStationSeries? windGust)
    {
        var latitude = temperature?.Latitude ?? windGust?.Latitude;
        var longitude = temperature?.Longitude ?? windGust?.Longitude;

        return new WeatherStationReading
        {
            StationId = stationId,
            StationName = temperature?.StationName ?? windGust?.StationName ?? stationId,
            Location = latitude is not null && longitude is not null
                ? new StationLocation(latitude.Value, longitude.Value)
                : null,
            Temperature = ToParameterReadings(temperature),
            WindGust = ToParameterReadings(windGust),
        };
    }

    private static IReadOnlyList<ParameterReading> ToParameterReadings(SmhiStationSeries? series)
    {
        if (series is null)
        {
            return [];
        }

        return series.Measurements
            .Select(m => new ParameterReading(m.Value, series.Unit, m.MeasuredAt, m.Quality))
            .ToList();
    }
}
