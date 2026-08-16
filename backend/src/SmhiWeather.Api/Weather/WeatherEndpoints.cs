using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmhiWeather.Application.Smhi;
using SmhiWeather.Application.Weather;

namespace SmhiWeather.Api.Weather;

public static class WeatherEndpoints
{
    public static IEndpointRouteBuilder MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/weather-readings")
            .RequireAuthorization()
            .WithTags("Weather");

        group.MapGet("/", GetReadingsAsync)
            .WithName("GetWeatherReadings")
            .WithSummary("Get combined Lufttemperatur and Byvind readings from SMHI.")
            .WithDescription(
                "Defaults to all stations for the latest hour. Filter with 'stationId' for a single " +
                "station, and 'period' ('hour' or 'day', default 'hour') for how far back to look.")
            .Produces<IReadOnlyList<WeatherStationReadingResponse>>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<Results<Ok<IReadOnlyList<WeatherStationReadingResponse>>, ValidationProblem>> GetReadingsAsync(
        [FromQuery] string? stationId,
        [FromQuery] string? period,
        IWeatherReadingsService weatherReadingsService,
        CancellationToken cancellationToken)
    {
        if (!TryParsePeriod(period, out var parsedPeriod))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["period"] = ["Value must be 'hour' or 'day'."],
            });
        }

        var readings = await weatherReadingsService.GetReadingsAsync(stationId, parsedPeriod, cancellationToken);
        var response = readings.Select(WeatherStationReadingResponse.FromDomain).ToList();

        return TypedResults.Ok<IReadOnlyList<WeatherStationReadingResponse>>(response);
    }

    private static bool TryParsePeriod(string? period, out WeatherPeriod result)
    {
        switch (period?.Trim().ToLowerInvariant())
        {
            case null or "":
            case "hour":
                result = WeatherPeriod.Hour;
                return true;
            case "day":
                result = WeatherPeriod.Day;
                return true;
            default:
                result = default;
                return false;
        }
    }
}
