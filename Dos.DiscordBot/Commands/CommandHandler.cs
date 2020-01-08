using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dos.DiscordBot.Commands
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider serviceProvider;
        private readonly GameRouterService gameRouterService;
        private readonly ILogger logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
        {
            this.client = client;
            this.commands = commands;
            this.serviceProvider = serviceProvider;
            gameRouterService = serviceProvider.GetService<GameRouterService>();
            logger = serviceProvider.GetService<ILogger>();
        }

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;

            var argPos = 0;
            if (!message.HasStringPrefix("dos", ref argPos, StringComparison.InvariantCultureIgnoreCase) ||
                message.Author.IsBot)
                return;

            var context = new DosCommandContext(client, message)
            {
                DosGame = gameRouterService.TryFindGameByChannel(message.Channel)
            };

            try
            {
                await commands.ExecuteAsync(context, 0, serviceProvider);
            }
            catch (Exception e)
            {
                logger.Error(e, "An error occured during execution of a command:");
                throw;
            }
        }
    }
}
