using Microsoft.Extensions.DependencyInjection;

namespace Govor.ConsoleClient;

internal class Program
{
    //private const string HubBaseUrl = "https://govor-team-govor-88b3.twc1.net/hubs";

    static async Task Main()
    {
        Console.Title = "Govor Console Client";

        var serviceProvider = DependencyInjection.Configure();

        var app = ActivatorUtilities.CreateInstance<App>(serviceProvider);
        await app.RunAsync();
    }
}