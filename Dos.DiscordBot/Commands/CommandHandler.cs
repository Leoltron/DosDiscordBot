using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dos.DiscordBot.Util;
using Dos.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dos.DiscordBot.Commands
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly GameRouterService gameRouterService;
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

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
            commands.CommandExecuted += OnCommandExecutedAsync;
            commands.Log += Log;
            client.MessageReceived += HandleCommandAsync;
            client.ChannelDestroyed += c =>
            {
                if (c is ISocketMessageChannel smc)
                    gameRouterService.TryDeleteGame(smc);
                return Task.CompletedTask;
            };

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
        }

        private Task Log(LogMessage msg)
        {
            logger.Write(msg.Severity.ToLogLevel(), msg.Exception, $"[{msg.Source}] {msg.Message}");
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message))
                return;

            var argPos = 0;
            if (!message.HasStringPrefix("dos", ref argPos, StringComparison.InvariantCultureIgnoreCase) ||
                message.Author.IsBot)
                return;

            while (argPos < message.Content.Length && message.Content[argPos] == ' ')
                argPos++;

            var context = new DosCommandContext(client, message)
            {
                DosGame = gameRouterService.TryFindGameByChannel(message.Channel)
            };

            int nextArgPos;
            if ((nextArgPos = message.Content.IndexOf("&&", StringComparison.InvariantCulture)) != -1)
            {
                nextArgPos += 2;
                while (nextArgPos < message.Content.Length && message.Content[nextArgPos] == ' ')
                    nextArgPos++;
                context.NextCommandArgPos = nextArgPos;
            }

            await commands.ExecuteAsync(context, argPos, serviceProvider);
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context,
                                                  IResult result)
        {
            var commandName = command.IsSpecified ? command.Value.Name : "A command";

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                if (result.Error != CommandError.Exception && !result.ErrorReason.IsNullOrEmpty())
                    await context.Channel.SendMessageAsync(result.ErrorReason);
            }
            else
            {
                logger.Information($"[{context.Guild?.Name ?? "DM"} - #{context.Channel.Name}] " +
                                   $"{commandName} was executed by {context.User.DiscordTag()}.");
            }

            if (context is DosCommandContext dosContext)
            {
                var nextArgPos = dosContext.NextCommandArgPos;
                if (result.IsSuccess && nextArgPos.HasValue)
                {
                    dosContext.NextCommandArgPos = null;
                    await commands.ExecuteAsync(dosContext, nextArgPos.Value, serviceProvider);
                }
            }
        }
    }
}
