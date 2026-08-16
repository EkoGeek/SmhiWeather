using SmhiWeather.Application.Smhi;

namespace SmhiWeather.IntegrationTests;

/// <summary>
/// Replaces the real SMHI client in integration tests so requests never hit the live SMHI API.
/// Returns a single measurement for period=hour and a short multi-hour series for period=day,
/// mirroring how the real SMHI API behaves.
/// </summary>
internal sealed class StubSmhiClient : ISmhiClient
{
    private static readonly DateTimeOffset MeasuredAt = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    public Task<SmhiParameterDataset> GetLufttemperaturAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken) =>
        Task.FromResult(Series("celsius", period == WeatherPeriod.Day ? [26.5, 25.1, 18.4] : [18.4]));

    public Task<SmhiParameterDataset> GetByvindAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken) =>
        Task.FromResult(Series("meter per sekund", period == WeatherPeriod.Day ? [4.0, 6.2, 9.7] : [9.7]));

    public Task<SmhiParameterDataset> GetMedelvindAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken) =>
        Task.FromResult(Series("meter per sekund", period == WeatherPeriod.Day ? [1.5, 2.8, 5.1] : [5.1]));

    private static SmhiParameterDataset Series(string unit, double[] values) => new(
    [
        new SmhiStationSeries(
            "98230",
            "Stockholm-Observatoriekullen A",
            59.34,
            18.05,
            unit,
            values.Select((v, i) => new SmhiMeasurement(v, MeasuredAt.AddHours(i), "G")).ToList()),
    ]);
}
