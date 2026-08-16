using SmhiWeather.Domain;

namespace SmhiWeather.Api.Weather;

/// <summary>
/// Temperature/WindGust hold one entry per SMHI-reported measurement for the requested period -
/// one for period=hour, up to ~24 for period=day. Either list may be empty.
/// </summary>
public sealed record WeatherStationReadingResponse(
    string StationId,
    string StationName,
    double? Latitude,
    double? Longitude,
    IReadOnlyList<ParameterReadingResponse> Temperature,
    IReadOnlyList<ParameterReadingResponse> WindGust)
{
    public static WeatherStationReadingResponse FromDomain(WeatherStationReading reading) => new(
        reading.StationId,
        reading.StationName,
        reading.Location?.Latitude,
        reading.Location?.Longitude,
        reading.Temperature.Select(ParameterReadingResponse.FromDomain).ToList(),
        reading.WindGust.Select(ParameterReadingResponse.FromDomain).ToList());
}
