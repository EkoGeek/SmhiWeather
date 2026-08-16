namespace SmhiWeather.Domain;

/// <summary>
/// Combined weather data for a single station. Either parameter may be absent if the station
/// does not measure it, or reported no value for the requested period.
/// </summary>
public sealed class WeatherStationReading
{
    public required string StationId { get; init; }

    public required string StationName { get; init; }

    public StationLocation? Location { get; init; }

    public ParameterReading? Temperature { get; init; }

    public ParameterReading? WindGust { get; init; }
}
