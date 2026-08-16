using System.Text.Json.Serialization;

namespace SmhiWeather.Infrastructure.Smhi.Dtos;

internal sealed class SmhiValueDto
{
    [JsonPropertyName("date")]
    public long Date { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }

    [JsonPropertyName("quality")]
    public string? Quality { get; init; }
}
