using SmhiWeather.Application.Smhi;
using SmhiWeather.Domain;

namespace SmhiWeather.Application.Weather;

public sealed class WeatherReadingsService(ISmhiClient smhiClient) : IWeatherReadingsService
{
    public async Task<IReadOnlyList<WeatherStationReading>> GetReadingsAsync(
        string? stationId,
        WeatherPeriod period,
        CancellationToken cancellationToken)
    {
        var temperatureTask = smhiClient.GetLufttemperaturAsync(stationId, period, cancellationToken);
        var windGustTask = smhiClient.GetByvindAsync(stationId, period, cancellationToken);
        var windSpeedTask = smhiClient.GetMedelvindAsync(stationId, period, cancellationToken);

        await Task.WhenAll(temperatureTask, windGustTask, windSpeedTask);

        return WeatherReadingCombiner.Combine(
            temperatureTask.Result.Stations,
            windGustTask.Result.Stations,
            windSpeedTask.Result.Stations);
    }
}
