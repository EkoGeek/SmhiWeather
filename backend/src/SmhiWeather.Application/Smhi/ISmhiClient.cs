namespace SmhiWeather.Application.Smhi;

/// <summary>
/// Fetches raw parameter data from the SMHI open data API. Implemented in Infrastructure.
/// </summary>
public interface ISmhiClient
{
    /// <summary>Lufttemperatur (parameter 1).</summary>
    Task<SmhiParameterDataset> GetLufttemperaturAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken);

    /// <summary>Byvind (parameter 21).</summary>
    Task<SmhiParameterDataset> GetByvindAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken);
}
