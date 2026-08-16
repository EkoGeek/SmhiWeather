using SmhiWeather.Application.Smhi;
using SmhiWeather.Domain;

namespace SmhiWeather.Application.Weather;

/// <summary>
/// Merges independently-fetched Lufttemperatur, Byvind, and Medelvind datasets into one reading
/// per station. A station appearing in only some datasets is still included, with the others empty.
/// </summary>
public static class WeatherReadingCombiner
{
    public static IReadOnlyList<WeatherStationReading> Combine(
        IReadOnlyList<SmhiStationSeries> temperature,
        IReadOnlyList<SmhiStationSeries> windGust,
        IReadOnlyList<SmhiStationSeries> windSpeed)
    {
        var temperatureByStation = temperature.ToDictionary(series => series.StationId);
        var windGustByStation = windGust.ToDictionary(series => series.StationId);
        var windSpeedByStation = windSpeed.ToDictionary(series => series.StationId);
        var stationIds = temperatureByStation.Keys
            .Union(windGustByStation.Keys)
            .Union(windSpeedByStation.Keys);

        return stationIds
            .Select(stationId =>
            {
                temperatureByStation.TryGetValue(stationId, out var temperatureSeries);
                windGustByStation.TryGetValue(stationId, out var windGustSeries);
                windSpeedByStation.TryGetValue(stationId, out var windSpeedSeries);
                return ToStationReading(stationId, temperatureSeries, windGustSeries, windSpeedSeries);
            })
            .OrderBy(reading => reading.StationName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static WeatherStationReading ToStationReading(
        string stationId,
        SmhiStationSeries? temperature,
        SmhiStationSeries? windGust,
        SmhiStationSeries? windSpeed)
    {
        var latitude = temperature?.Latitude ?? windGust?.Latitude ?? windSpeed?.Latitude;
        var longitude = temperature?.Longitude ?? windGust?.Longitude ?? windSpeed?.Longitude;

        return new WeatherStationReading
        {
            StationId = stationId,
            StationName = temperature?.StationName ?? windGust?.StationName ?? windSpeed?.StationName ?? stationId,
            Location = latitude is not null && longitude is not null
                ? new StationLocation(latitude.Value, longitude.Value)
                : null,
            Temperature = ToParameterReadings(temperature),
            WindGust = ToParameterReadings(windGust),
            WindSpeed = ToParameterReadings(windSpeed),
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
