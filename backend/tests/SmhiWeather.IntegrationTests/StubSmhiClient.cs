using SmhiWeather.Application.Smhi;

namespace SmhiWeather.IntegrationTests;

/// <summary>
/// Replaces the real SMHI client in integration tests so requests never hit the live SMHI API.
/// </summary>
internal sealed class StubSmhiClient : ISmhiClient
{
    private static readonly DateTimeOffset MeasuredAt = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    public Task<SmhiParameterDataset> GetLufttemperaturAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken) =>
        Task.FromResult(new SmhiParameterDataset(
        [
            new SmhiStationValue("98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, 18.4, "celsius", MeasuredAt, "G"),
        ]));

    public Task<SmhiParameterDataset> GetByvindAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken) =>
        Task.FromResult(new SmhiParameterDataset(
        [
            new SmhiStationValue("98230", "Stockholm-Observatoriekullen A", 59.34, 18.05, 9.7, "meter per sekund", MeasuredAt, "G"),
        ]));
}
