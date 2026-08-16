using Microsoft.AspNetCore.Authentication;
using SmhiWeather.Api.ApiKeyAuth;
using SmhiWeather.Api.Weather;
using SmhiWeather.Application;
using SmhiWeather.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddOptions<ApiKeySettings>()
    .Bind(builder.Configuration.GetSection(ApiKeySettings.SectionName))
    .ValidateOnStart();

builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapWeatherEndpoints();

app.Run();

/// <summary>
/// Marker so integration tests can reference the entry point (<c>WebApplicationFactory&lt;Program&gt;</c>).
/// </summary>
public partial class Program;
