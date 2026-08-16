using SmhiWeather.Application.Smhi;
using SmhiWeather.Domain;

namespace SmhiWeather.Application.Weather;

/// <summary>
/// Merges independently-fetched Lufttemperatur and Byvind datasets into one reading per station.
/// A station that only appears in one dataset is still included, with the other parameter left null.
/// </summary>
public static class WeatherReadingCombiner
{
    public static IReadOnlyList<WeatherStationReading> Combine(
        IReadOnlyList<SmhiStationValue> temperature,
        IReadOnlyList<SmhiStationValue> windGust)
    {
        var temperatureByStation = temperature.ToDictionary(reading => reading.StationId);
        var windGustByStation = windGust.ToDictionary(reading => reading.StationId);
        var stationIds = temperatureByStation.Keys.Union(windGustByStation.Keys);

        return stationIds
            .Select(stationId =>
            {
                temperatureByStation.TryGetValue(stationId, out var temperatureReading);
                windGustByStation.TryGetValue(stationId, out var windGustReading);
                return ToStationReading(stationId, temperatureReading, windGustReading);
            })
            .OrderBy(reading => reading.StationName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static WeatherStationReading ToStationReading(
        string stationId,
        SmhiStationValue? temperature,
        SmhiStationValue? windGust)
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
            Temperature = temperature is null
                ? null
                : new ParameterReading(temperature.Value, temperature.Unit, temperature.MeasuredAt, temperature.Quality),
            WindGust = windGust is null
                ? null
                : new ParameterReading(windGust.Value, windGust.Unit, windGust.MeasuredAt, windGust.Quality),
        };
    }
}
