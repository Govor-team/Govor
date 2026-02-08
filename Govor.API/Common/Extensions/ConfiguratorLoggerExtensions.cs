using Serilog;

namespace Govor.API.Common.Extensions;

public static class ConfiguratorLoggerExtensions
{
    public static void AddLogger(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console() 
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Лог в файл, ежедневно
            .CreateLogger();
        
        builder.Host.UseSerilog();
    }
}