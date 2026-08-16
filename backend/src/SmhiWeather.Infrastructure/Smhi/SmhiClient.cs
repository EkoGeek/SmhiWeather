using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using SmhiWeather.Application.Smhi;
using SmhiWeather.Infrastructure.Smhi.Dtos;

namespace SmhiWeather.Infrastructure.Smhi;

public sealed class SmhiClient(HttpClient httpClient) : ISmhiClient
{
    private const string LufttemperaturParameter = "1";
    private const string ByvindParameter = "21";
    private const string MedelvindParameter = "4";

    public Task<SmhiParameterDataset> GetLufttemperaturAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken) =>
        GetParameterDatasetAsync(LufttemperaturParameter, stationId, period, cancellationToken);

    public Task<SmhiParameterDataset> GetByvindAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken) =>
        GetParameterDatasetAsync(ByvindParameter, stationId, period, cancellationToken);

    public Task<SmhiParameterDataset> GetMedelvindAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken) =>
        GetParameterDatasetAsync(MedelvindParameter, stationId, period, cancellationToken);

    private async Task<SmhiParameterDataset> GetParameterDatasetAsync(
        string parameter,
        string? stationId,
        WeatherPeriod period,
        CancellationToken cancellationToken)
    {
        var periodSegment = period == WeatherPeriod.Day ? "latest-day" : "latest-hour";
        var stationSegment = stationId is null ? "station-set/all" : $"station/{Uri.EscapeDataString(stationId)}";
        var requestUri = $"{parameter}/{stationSegment}/period/{periodSegment}/data.json";

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // The station doesn't report this parameter (or doesn't exist) - treat as "no data" rather than an error.
            return new SmhiParameterDataset([]);
        }

        response.EnsureSuccessStatusCode();

        return stationId is null
            ? MapStationSet(await ReadAsync<SmhiStationSetResponseDto>(response, cancellationToken))
            : MapSingleStation(await ReadAsync<SmhiSingleStationResponseDto>(response, cancellationToken));
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            ?? throw new InvalidOperationException($"SMHI returned an empty {typeof(T).Name} response.");
    }

    private static SmhiParameterDataset MapStationSet(SmhiStationSetResponseDto dto)
    {
        var unit = dto.Parameter.Unit;

        var stations = dto.Station
            .Select(station => MapSeries(station.Key, station.Name, station.Latitude, station.Longitude, unit, station.Value))
            .OfType<SmhiStationSeries>()
            .ToList();

        return new SmhiParameterDataset(stations);
    }

    private static SmhiParameterDataset MapSingleStation(SmhiSingleStationResponseDto dto)
    {
        var position = dto.Position.Count > 0 ? dto.Position[^1] : null;

        var series = MapSeries(
            dto.Station.Key,
            dto.Station.Name,
            position?.Latitude,
            position?.Longitude,
            dto.Parameter.Unit,
            dto.Value);

        return new SmhiParameterDataset(series is null ? [] : [series]);
    }

    /// <summary>Maps every valid value SMHI returned for a station (1 for "hour", up to ~24 for "day").</summary>
    private static SmhiStationSeries? MapSeries(
        string stationId,
        string stationName,
        double? latitude,
        double? longitude,
        string unit,
        List<SmhiValueDto> values)
    {
        var measurements = values
            .Select(TryMapMeasurement)
            .OfType<SmhiMeasurement>()
            .ToList();

        if (measurements.Count == 0)
        {
            return null;
        }

        return new SmhiStationSeries(stationId, stationName, latitude, longitude, unit, measurements);
    }

    private static SmhiMeasurement? TryMapMeasurement(SmhiValueDto value)
    {
        if (value.Value is null || !double.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return null;
        }

        return new SmhiMeasurement(parsedValue, DateTimeOffset.FromUnixTimeMilliseconds(value.Date), value.Quality ?? "");
    }
}
