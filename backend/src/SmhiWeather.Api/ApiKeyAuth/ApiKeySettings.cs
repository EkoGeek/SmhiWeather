namespace SmhiWeather.Api.ApiKeyAuth;

public sealed class ApiKeySettings
{
    public const string SectionName = "ApiKey";

    public required string Key { get; init; }
}
