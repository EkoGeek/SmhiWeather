using System.Text.Json.Serialization;

namespace SmhiWeather.Infrastructure.Smhi.Dtos;

internal sealed class SmhiParameterDto
{
    [JsonPropertyName("key")]
    public string Key { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("unit")]
    public string Unit { get; init; } = "";
}
