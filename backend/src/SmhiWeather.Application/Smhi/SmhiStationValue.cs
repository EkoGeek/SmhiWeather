namespace SmhiWeather.Application.Smhi;

/// <summary>One measured value at one point in time.</summary>
public sealed record SmhiMeasurement(double Value, DateTimeOffset MeasuredAt, string Quality);

/// <summary>
/// A single station's measurements for one SMHI parameter over the requested period, as returned
/// by <see cref="ISmhiClient"/>. Ordered oldest to newest.
/// </summary>
public sealed record SmhiStationSeries(
    string StationId,
    string StationName,
    double? Latitude,
    double? Longitude,
    string Unit,
    IReadOnlyList<SmhiMeasurement> Measurements);

public sealed record SmhiParameterDataset(IReadOnlyList<SmhiStationSeries> Stations);
