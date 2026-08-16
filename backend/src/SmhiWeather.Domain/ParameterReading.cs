namespace SmhiWeather.Domain;

/// <summary>
/// A single measured value for one parameter (e.g. temperature or wind gust) at a station.
/// </summary>
public sealed record ParameterReading(double Value, string Unit, DateTimeOffset MeasuredAt, string Quality);
