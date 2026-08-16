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

    /// <summary>Medelvind / Vindhastighet - 10-minute mean wind speed (parameter 4).</summary>
    Task<SmhiParameterDataset> GetMedelvindAsync(string? stationId, WeatherPeriod period, CancellationToken cancellationToken);
}
