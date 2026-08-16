namespace SmhiWeather.Application.Smhi;

/// <summary>
/// A single station's latest value for one SMHI parameter, as returned by <see cref="ISmhiClient"/>.
/// </summary>
public sealed record SmhiStationValue(
    string StationId,
    string StationName,
    double? Latitude,
    double? Longitude,
    double Value,
    string Unit,
    DateTimeOffset MeasuredAt,
    string Quality);

public sealed record SmhiParameterDataset(IReadOnlyList<SmhiStationValue> Stations);
