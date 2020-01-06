using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dos.DiscordBot
{
    public static class ServiceProviderBuilder
    {
        public static IServiceProvider BuildProvider(ILogger loggerInstance) =>
            new ServiceCollection()
               .AddSingleton<GameRouterService>()
               .AddSingleton(loggerInstance)
               .BuildServiceProvider();
    }
}
