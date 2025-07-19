using Govor.ConsoleClient.Services.Implementations;
using Govor.ConsoleClient.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Govor.ConsoleClient.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IInputPipeline, InputPipeline>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IMiddlewarePipeline, MiddlewarePipeline>();
        services.AddSingleton<ConsoleLogger>();
        services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ConsoleLogger>());
        return services;
    }
}