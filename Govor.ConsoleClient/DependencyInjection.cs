using System.Reflection;
using Govor.ConsoleClient.Commands;
using Govor.ConsoleClient.Services;
using Govor.ConsoleClient.Services.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Govor.ConsoleClient;

public static class DependencyInjection
{
    public static IServiceProvider Configure()
    {
        var services = new ServiceCollection();

        // Регистрация команд
        services.AddCommands();
        
        // Сервисы
        services.AddSingleton<CommandDispatcher>();
        services.AddSingleton<InputPipeline>();
        services.AddSingleton<ConsoleLogger>();
        services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ConsoleLogger>());
        services.AddSingleton<CommandContext>();
        services.AddSingleton<MiddlewarePipeline>();
        
        // Middleware
        services.AddSingleton<ICommandMiddleware, ExceptionHandlingMiddleware>();
        
        
        return services.BuildServiceProvider();
    }
    
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        var commandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in commandTypes)
        {
            services.AddTransient(typeof(ICommand), type);
            services.AddTransient(type); // Для прямого внедрения
        }

        return services;
    }
}