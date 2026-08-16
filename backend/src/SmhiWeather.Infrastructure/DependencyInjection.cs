using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmhiWeather.Application.Smhi;
using SmhiWeather.Infrastructure.Smhi;

namespace SmhiWeather.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<SmhiApiOptions>()
            .Bind(configuration.GetSection(SmhiApiOptions.SectionName))
            .ValidateOnStart();

        var baseUrl = configuration[$"{SmhiApiOptions.SectionName}:BaseUrl"]
            ?? throw new InvalidOperationException($"Configuration value '{SmhiApiOptions.SectionName}:BaseUrl' is not configured.");

        services.AddHttpClient<ISmhiClient, SmhiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
