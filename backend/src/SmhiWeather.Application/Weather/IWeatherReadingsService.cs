using SmhiWeather.Application.Smhi;
using SmhiWeather.Domain;

namespace SmhiWeather.Application.Weather;

public interface IWeatherReadingsService
{
    /// <summary>
    /// Gets combined temperature and wind gust readings. When <paramref name="stationId"/> is null,
    /// returns readings for all stations.
    /// </summary>
    Task<IReadOnlyList<WeatherStationReading>> GetReadingsAsync(
        string? stationId,
        WeatherPeriod period,
        CancellationToken cancellationToken);
}
