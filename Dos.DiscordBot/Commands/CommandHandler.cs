using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dos.Utils;
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
            commands.CommandExecuted += OnCommandExecutedAsync;
            client.ChannelDestroyed += c =>
            {
                if (c is ISocketMessageChannel smc)
                    gameRouterService.TryDeleteGame(smc);
                return Task.CompletedTask;
            };

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;

            var argPos = 0;
            if (!message.HasStringPrefix("dos", ref argPos, StringComparison.InvariantCultureIgnoreCase) ||
                message.Author.IsBot)
                return;

            if (argPos < message.Content.Length && message.Content[argPos] == ' ')
            {
                argPos++;
            }

            var context = new DosCommandContext(client, message)
            {
                DosGame = gameRouterService.TryFindGameByChannel(message.Channel)
            };

            try
            {
                await commands.ExecuteAsync(context, argPos, serviceProvider);
                if ((argPos = message.Content.IndexOf("&&", StringComparison.InvariantCulture)) != -1)
                {
                    await commands.ExecuteAsync(context, argPos+2, serviceProvider);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "An error occured during execution of a command:");
                throw;
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context,
                                                  IResult result)
        {
            if (result.Error != CommandError.UnknownCommand && !(result?.ErrorReason).IsNullOrEmpty())
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

            var commandName = command.IsSpecified ? command.Value.Name : "A command";
            logger.Information(
                $"[{context.Guild.Name} #{context.Channel.Name}] {commandName} was executed at {DateTime.UtcNow}.");
        }
    }
}
