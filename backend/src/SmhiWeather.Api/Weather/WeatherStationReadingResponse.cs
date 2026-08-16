using SmhiWeather.Domain;

namespace SmhiWeather.Api.Weather;

public sealed record WeatherStationReadingResponse(
    string StationId,
    string StationName,
    double? Latitude,
    double? Longitude,
    ParameterReadingResponse? Temperature,
    ParameterReadingResponse? WindGust)
{
    public static WeatherStationReadingResponse FromDomain(WeatherStationReading reading) => new(
        reading.StationId,
        reading.StationName,
        reading.Location?.Latitude,
        reading.Location?.Longitude,
        reading.Temperature is null ? null : ParameterReadingResponse.FromDomain(reading.Temperature),
        reading.WindGust is null ? null : ParameterReadingResponse.FromDomain(reading.WindGust));
}
