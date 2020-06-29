using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dos.Database.Models;
using Dos.DiscordBot.Util;
using Serilog;

namespace Dos.DiscordBot.Helpers
{
    public class GameConfigHelper : IGameConfigHelper
    {
        private readonly IGameConfigRepository gameConfigRepository;
        private readonly ILogger logger;

        public GameConfigHelper(IGameConfigRepository gameConfigRepository, ILogger logger)
        {
            this.gameConfigRepository = gameConfigRepository;
            this.logger = logger;
        }

        public async Task SendGameConfigAsync(DiscordDosGame game)
        {
            var message = "Current game configuration:\n" + game.Config.ToDiscordTable();
            await game.Info.Channel.SendMessageAsync(message);
        }

        public async Task SendServerDefaultGameConfigAsync(SocketCommandContext context)
        {
            var config = await gameConfigRepository.GetConfigOrDefaultAsync(context.Guild.Id);
            await SendServerDefaultGameConfigAsync(config, context.Channel);
        }

        private static async Task SendServerDefaultGameConfigAsync(BotGameConfig config, ISocketMessageChannel channel)
        {
            var message = "Server default game configuration:\n" + config.ToDiscordTable();
            await channel.SendMessageAsync(message);
        }

        public async Task SetGameConfigAsync(SocketCommandContext context, string key, string value)
        {
            var game = context.GetGame();
            if (game.Owner.Id != context.User.Id)
            {
                await context.Channel.SendMessageAsync(
                    $"Sorry, but only game's owner **{game.Owner.Username}** can change config");
                return;
            }

            var result = game.Config.Set(key, value);
            if (result.IsFail)
                await context.Channel.SendMessageAsync(result.Message);
            else
                await SendGameConfigAsync(game);
        }

        public async Task SetServerDefaultGameConfigAsync(SocketCommandContext context, string key, string value)
        {
            if (!(context.User is IGuildUser guildUser))
                return;

            if (!guildUser.GuildPermissions.ManageRoles)
            {
                await context.Channel.SendMessageAsync(
                    "Sorry, you need to have \"Manage Roles\" permission to change server default config");
                return;
            }
            
            var config = await gameConfigRepository.GetConfigOrDefaultAsync(context.Guild.Id);
            var result = config.Set(key, value);
            if (result.IsFail)
                await context.Channel.SendMessageAsync(result.Message);
            else
            {
                try
                {
                    await gameConfigRepository.UpdateGameConfigAsync(context.Guild.Id, config);
                }
                catch (Exception e)
                {
                    logger.Error(e, "An exception occurred during config update:");
                    await context.Channel.SendMessageAsync("An error occurred during config update");
                    return;
                }

                await SendServerDefaultGameConfigAsync(config, context.Channel);
            }
        }
    }
}
