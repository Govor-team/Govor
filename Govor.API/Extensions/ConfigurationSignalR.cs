using Govor.API.Filters;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Extensions;

public static class ConfigurationSignalR
{
    public static void AddSignalRConf(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.AddFilter<HubExceptionFilter>();
        });
    }
}