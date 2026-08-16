using SmhiWeather.Application.Smhi;

namespace SmhiWeather.UnitTests.Weather;

internal sealed class FakeSmhiClient(SmhiParameterDataset temperature, SmhiParameterDataset windGust) : ISmhiClient
{
    public string? RequestedStationId { get; private set; }

    public WeatherPeriod? RequestedPeriod { get; private set; }

    public Task<SmhiParameterDataset> GetLufttemperaturAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken)
    {
        RequestedStationId = stationId;
        RequestedPeriod = period;
        return Task.FromResult(temperature);
    }

    public Task<SmhiParameterDataset> GetByvindAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken)
    {
        RequestedStationId = stationId;
        RequestedPeriod = period;
        return Task.FromResult(windGust);
    }
}
