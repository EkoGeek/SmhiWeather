using SmhiWeather.Domain;

namespace SmhiWeather.Api.Weather;

public sealed record ParameterReadingResponse(double Value, string Unit, DateTimeOffset MeasuredAt, string Quality)
{
    public static ParameterReadingResponse FromDomain(ParameterReading reading) =>
        new(reading.Value, reading.Unit, reading.MeasuredAt, reading.Quality);
}
