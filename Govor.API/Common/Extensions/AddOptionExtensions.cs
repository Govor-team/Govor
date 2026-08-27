using Govor.Application.Authentication.JWT;
using Govor.Application.Messages;

namespace Govor.API.Common.Extensions;

public static class AddOptionExtensions
{
    public static IServiceCollection AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtAccessOption>(configuration.GetSection(nameof(JwtAccessOption)));
        services.Configure<JwtRefreshOption>(configuration.GetSection(nameof(JwtRefreshOption)));
        services.Configure<MessageEditingOptions>(configuration.GetSection(nameof(MessageEditingOptions)));
        return services;
    }
}