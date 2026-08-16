using System.Text.Json.Serialization;

namespace SmhiWeather.Infrastructure.Smhi.Dtos;

/// <summary>
/// Shape returned by .../parameter/{p}/station-set/all/period/{period}/data.json.
/// Each station entry carries its own lat/long and value array.
/// </summary>
internal sealed class SmhiStationSetResponseDto
{
    [JsonPropertyName("parameter")]
    public SmhiParameterDto Parameter { get; init; } = new();

    [JsonPropertyName("station")]
    public List<SmhiStationSetItemDto> Station { get; init; } = [];
}

internal sealed class SmhiStationSetItemDto
{
    [JsonPropertyName("key")]
    public string Key { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("latitude")]
    public double? Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; init; }

    [JsonPropertyName("value")]
    public List<SmhiValueDto> Value { get; init; } = [];
}
