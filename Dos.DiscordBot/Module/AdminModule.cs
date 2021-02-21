using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Dos.Database;
using Dos.Database.Models;
using Dos.DiscordBot.Attributes;
using Dos.DiscordBot.Util;
using Dos.Utils;

namespace Dos.DiscordBot.Module
{
    [AdminOnly]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class AdminModule : ExtendedModule
    {
        private readonly GameRouterService gameRouterService;
        private readonly DosDbContext dosDbContext;

        public AdminModule(GameRouterService gameRouterService, DosDbContext dosDbContext)
        {
            this.gameRouterService = gameRouterService;
            this.dosDbContext = dosDbContext;
        }

        [NoDm]
        [Command("addgame")]
        public async Task AddGame()
        {
            if (Context.Guild == null)
                return;

            var guildId = Context.Guild.Id;
            if (dosDbContext.GuildConfig.Any(c => c.GuildId == guildId))
            {
                await Context.Channel.SendMessageAsync("Already found something for this server");
                return;
            }
            await dosDbContext.GuildConfig.AddAsync(new GuildConfig
            {
                Config = new BotGameConfig(),
                GuildId = guildId
            });
            await dosDbContext.SaveChangesAsync();
            await Context.Channel.SendMessageAsync("Done.");
        }

        [Command("games")]
        public Task RunningGames()
        {
            var now = DateTime.UtcNow;
            var runningGames = gameRouterService.GetRunningGamesInfo().ToList();

            if (runningGames.IsEmpty())
                return Context.Channel.SendMessageAsync("No running games right now.");

            var message = new StringBuilder($"Currently running games ({runningGames.Count}):");
            foreach (var gameInfo in runningGames)
            {
                var time = now - gameInfo.CreateDate;
                var timeString = time > TimeSpan.FromDays(1) ? "**More than a day**" : $"{time:hh\\:mm\\:ss}";

                message.Append(
                    $"\n\t[{timeString}] {gameInfo.ServerName}, #{gameInfo.Channel.Name}, owner: {gameInfo.Owner.DiscordTag()}");
            }

            return Context.Channel.SendMessageAsync(message.ToString());
        }

        [Command("stop-bot")]
        public async Task StopStartingGames()
        {
            if (!gameRouterService.PreventStartNewGames)
            {
                gameRouterService.NoNewGames();
                await Context.Client.SetStatusAsync(UserStatus.DoNotDisturb);
                if (gameRouterService.GetActiveGamesCount == 0)
                {
                    await Context.Channel.SendMessageAsync(
                        "No active games right now, attempts to start any new will be blocked. " +
                        "You can safely stop bot now.");
                    return;
                }

                gameRouterService.LastGameEnded += () =>
                    Context.User.SendMessageAsync("Last game has ended. You may safely stop bot now.");
                await Context.Channel.SendMessageAsync("Any request to start new game will be rejected from now on.");
            }

            await Context.Channel.SendMessageAsync($"{gameRouterService.GetActiveGamesCount} active games right now.");
        }
    }
}
