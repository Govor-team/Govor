using Govor.Application.Services.Authentication;

namespace Govor.API.Extensions;

public static class AddOptionExtensions
{
    public static IServiceCollection AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtAccessOption>(configuration.GetSection(nameof(JwtAccessOption)));
        services.Configure<JwtRefreshOption>(configuration.GetSection(nameof(JwtRefreshOption)));

        return services;
    }
}