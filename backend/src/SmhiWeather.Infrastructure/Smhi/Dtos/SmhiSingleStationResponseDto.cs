using System.Text.Json.Serialization;

namespace SmhiWeather.Infrastructure.Smhi.Dtos;

/// <summary>
/// Shape returned by .../parameter/{p}/station/{id}/period/{period}/data.json.
/// The station is a single object; position (with lat/long) is a separate top-level array.
/// </summary>
internal sealed class SmhiSingleStationResponseDto
{
    [JsonPropertyName("parameter")]
    public SmhiParameterDto Parameter { get; init; } = new();

    [JsonPropertyName("station")]
    public SmhiSingleStationDto Station { get; init; } = new();

    [JsonPropertyName("position")]
    public List<SmhiPositionDto> Position { get; init; } = [];

    [JsonPropertyName("value")]
    public List<SmhiValueDto> Value { get; init; } = [];
}

internal sealed class SmhiSingleStationDto
{
    [JsonPropertyName("key")]
    public string Key { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";
}

internal sealed class SmhiPositionDto
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }
}
