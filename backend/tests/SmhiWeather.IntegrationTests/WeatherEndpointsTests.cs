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
        Assert.NotNull(reading.Temperature);
        Assert.NotNull(reading.WindGust);
    }

    [Fact]
    public async Task GetWeatherReadings_InvalidPeriod_ReturnsValidationProblem()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, WeatherApiFactory.ValidApiKey);

        var response = await client.GetAsync("/api/weather-readings?period=week");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
