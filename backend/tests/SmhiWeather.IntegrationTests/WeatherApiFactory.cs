using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmhiWeather.Application.Smhi;

namespace SmhiWeather.IntegrationTests;

public sealed class WeatherApiFactory : WebApplicationFactory<Program>
{
    public const string ValidApiKey = "test-api-key";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey:Key"] = ValidApiKey,
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ISmhiClient>();
            services.AddSingleton<ISmhiClient, StubSmhiClient>();
        });
    }
}
