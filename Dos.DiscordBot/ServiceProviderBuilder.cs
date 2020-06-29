using System;
using Dos.Database;
using Dos.DiscordBot.Helpers;
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
               .AddTransient<IGameConfigHelper, GameConfigHelper>()
               .AddTransient<IGameConfigRepository, GameConfigRepository>()
               .AddDbContext<BotDbContext>()
               .BuildServiceProvider();
    }
}
