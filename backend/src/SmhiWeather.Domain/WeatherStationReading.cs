namespace SmhiWeather.Domain;

/// <summary>
/// Combined weather data for a single station. Temperature/WindGust hold one entry per
/// SMHI-reported measurement for the requested period (one for "hour", up to ~24 for "day").
/// Either list may be empty if the station does not measure that parameter.
/// </summary>
public sealed class WeatherStationReading
{
    public required string StationId { get; init; }

    public required string StationName { get; init; }

    public StationLocation? Location { get; init; }

    public required IReadOnlyList<ParameterReading> Temperature { get; init; }

    public required IReadOnlyList<ParameterReading> WindGust { get; init; }
}
