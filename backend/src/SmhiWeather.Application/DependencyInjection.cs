using Microsoft.Extensions.DependencyInjection;
using SmhiWeather.Application.Weather;

namespace SmhiWeather.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWeatherReadingsService, WeatherReadingsService>();

        return services;
    }
}
