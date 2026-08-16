using System.Net;
using System.Net.Http.Json;
using SmhiWeather.Api.ApiKeyAuth;
using SmhiWeather.Api.Weather;

namespace SmhiWeather.IntegrationTests;

public class WeatherEndpointsTests(WeatherApiFactory factory) : IClassFixture<WeatherApiFactory>
{
    [Fact]
    public async Task GetWeatherReadings_MissingApiKey_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/weather-readings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWeatherReadings_InvalidApiKey_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, "wrong-key");

        var response = await client.GetAsync("/api/weather-readings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWeatherReadings_ValidApiKey_ReturnsCombinedReadings()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, WeatherApiFactory.ValidApiKey);

        var response = await client.GetAsync("/api/weather-readings?stationId=98230&period=hour");

        response.EnsureSuccessStatusCode();
        var readings = await response.Content.ReadFromJsonAsync<List<WeatherStationReadingResponse>>();
        var reading = Assert.Single(readings!);
        Assert.Equal("98230", reading.StationId);
        Assert.Single(reading.Temperature);
        Assert.Single(reading.WindGust);
        Assert.Single(reading.WindSpeed);
    }

    [Fact]
    public async Task GetWeatherReadings_DayPeriodWithStationId_ReturnsFullSeriesPerParameter()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, WeatherApiFactory.ValidApiKey);

        var response = await client.GetAsync("/api/weather-readings?stationId=98230&period=day");

        response.EnsureSuccessStatusCode();
        var readings = await response.Content.ReadFromJsonAsync<List<WeatherStationReadingResponse>>();
        var reading = Assert.Single(readings!);
        Assert.Equal([26.5, 25.1, 18.4], reading.Temperature.Select(r => r.Value));
        Assert.Equal([4.0, 6.2, 9.7], reading.WindGust.Select(r => r.Value));
        Assert.Equal([1.5, 2.8, 5.1], reading.WindSpeed.Select(r => r.Value));
    }

    [Fact]
    public async Task GetWeatherReadings_InvalidPeriod_ReturnsValidationProblem()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, WeatherApiFactory.ValidApiKey);

        var response = await client.GetAsync("/api/weather-readings?period=week");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWeatherReadings_DayPeriodWithoutStationId_ReturnsValidationProblem()
    {
        // SMHI has no all-stations feed for period=latest-day; only per-station queries support it.
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, WeatherApiFactory.ValidApiKey);

        var response = await client.GetAsync("/api/weather-readings?period=day");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
