using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dos.DiscordBot
{
    public static class ServiceProviderBuilder
    {
        public static IServiceProvider BuildProvider() =>
            new ServiceCollection()
               .AddSingleton<GameProviderService>()
               .BuildServiceProvider();
    }
}
