namespace SmhiWeather.Infrastructure.Smhi;

public sealed class SmhiApiOptions
{
    public const string SectionName = "Smhi";

    /// <summary>e.g. https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/ (must end with '/').</summary>
    public required string BaseUrl { get; init; }
}
