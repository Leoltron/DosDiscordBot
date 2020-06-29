using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Dos.DiscordBot.Attributes;
using Dos.DiscordBot.Helpers;
using Dos.DiscordBot.Util;

namespace Dos.DiscordBot.Module
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [NoDm]
    public class GameModule : ExtendedModule
    {
        private readonly GameRouterService gameRouterService;
        private readonly IGameConfigHelper gameConfigHelper;

        public GameModule(GameRouterService gameRouterService, IGameConfigHelper gameConfigHelper)
        {
            this.gameRouterService = gameRouterService;
            this.gameConfigHelper = gameConfigHelper;
        }

        private DiscordDosGame Game => Context.GetGame();

        [Command("join", true)]
        [Alias("j")]
        public async Task JoinGame()
        {
            await ReplyIfHasMessageAsync(
                await gameRouterService.JoinGameAsync(Context.Guild, Context.Channel, Context.User));
        }

        [Command("quit", true)]
        [Alias("q")]
        public Task QuitGame() => ReplyIfHasMessageAsync(gameRouterService.Quit(Context.Channel, Context.User));

        [NotStartedGameRequired]
        [Command("start", true)]
        [Alias("s")]
        public async Task StartGame()
        {
            await ReplyIfHasMessageAsync(await Game.StartAsync(Context.User));
        }

        [StartedGameRequired]
        [Command("draw", true)]
        [Alias("take")]
        public async Task Draw()
        {
            await ReplyIfHasMessageAsync(await Game.DrawAsync(Context.User));
        }

        [StartedGameRequired]
        [Command("done", true)]
        [Alias("end")]
        public async Task EndTurn()
        {
            await ReplyIfHasMessageAsync(await Game.EndTurnAsync(Context.User));
        }

        [StartedGameRequired]
        [Command("select")]
        [Alias("choose")]
        public async Task Select([Remainder] string args)
        {
            await ReplyIfHasMessageAsync(await Game.SelectAsync(Context.User, args.Split("&&").First()));
        }

        [StartedGameRequired]
        [Command("match", true)]
        [Alias("m")]
        public async Task Match([Remainder] string args)
        {
            await ReplyIfHasMessageAsync(await Game.MatchAsync(Context.User, args.Split("&&").First()));
        }

        [StartedGameRequired]
        [Command("add", true)]
        [Alias("a")]
        public async Task Add([Remainder] string args)
        {
            await ReplyIfHasMessageAsync(await Game.AddToCenterRowAsync(Context.User, args.Split("&&").First()));
        }

        [StartedGameRequired]
        [Command("hand", true)]
        [Alias("h")]
        public async Task Hand()
        {
            await ReplyIfHasMessageAsync(await Game.SendHandAsync(Context.User, true));
        }

        [StartedGameRequired]
        [Command("swap", true)]
        [Alias("switch", "trade", "exchange")]
        public async Task SwapByMention(IUser target)
        {
            await ReplyIfHasMessageAsync(await Game.SwapAsync(Context.User, target));
        }

        [StartedGameRequired]
        [Command("swap", true)]
        [Alias("switch", "trade", "exchange")]
        public async Task SwapByName(string targetName)
        {
            await ReplyIfHasMessageAsync(await Game.SwapAsync(Context.User, targetName));
        }

        [StartedGameRequired]
        [Command("table", true)]
        [Alias("t")]
        public Task Table() => Game.SendTableToChannel(true);

        [StartedGameRequired]
        [Command("!", true)]
        public async Task CallDos() => await ReplyIfHasMessageAsync(await Game.CallDosAsync(Context.User));

        [StartedGameRequired]
        [Command("callout", true)]
        public async Task Callout() => await ReplyIfHasMessageAsync(await Game.CalloutAsync(Context.User));

        [CreatedGameRequired]
        [Command("config")]
        public async Task GetConfig() => await gameConfigHelper.SendGameConfigAsync(Game);

        [NoGameRequired]
        [Command("config")]
        public async Task GetServerConfig() => await gameConfigHelper.SendServerDefaultGameConfigAsync(Context);

        [Command("config")]
        public async Task GetConfigDescription(string key) =>
            await Context.Channel.SendMessageAsync(BotGameConfigExtensions.GetDescription(key));

        [NotStartedGameRequired]
        [Command("config")]
        public async Task SetConfig(string key, string value) =>
            await gameConfigHelper.SetGameConfigAsync(Context, key, value);

        [NoGameRequired]
        [Command("config")]
        public async Task SetServerConfig(string key, string value) =>
            await gameConfigHelper.SetServerDefaultGameConfigAsync(Context, key, value);

        [NotStartedGameRequired]
        [Command("add-bot", true)]
        public async Task AddBot() => await ReplyIfHasMessageAsync(await Game.AddBotAsync(Context.User));

        [StartedGameRequired]
        [Command("stop", true)]
        [Alias("end", "endgame", "stopgame")]
        public async Task StopGame()
        {
            await ReplyIfHasMessageAsync(await Game.StopAsync(Context.User));
        }
    }
}
