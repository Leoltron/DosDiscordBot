using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.Commands;
using Dos.DiscordBot.Attributes;
using Dos.DiscordBot.Util;

namespace Dos.DiscordBot.Module
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class GameModule : ExtendedModule
    {
        private readonly GameRouterService gameRouterService;

        public GameModule(GameRouterService gameRouterService)
        {
            this.gameRouterService = gameRouterService;
        }

        private DiscordDosGame Game => Context.GetGame();

        [Command("dos join", true), Alias("dosjoin", "dosj")]
        public async Task JoinGame()
        {
            await ReplyIfHasMessageAsync(
                await gameRouterService.JoinGameAsync(Context.Guild, Context.Channel, Context.User));
        }

        [NotStartedGameRequired]
        [Command("dos start", true), Alias("dosstart", "doss")]
        public async Task StartGame()
        {
            await ReplyIfHasMessageAsync(await Game.StartAsync(Context.User));
        }

        [StartedGameRequired]
        [Command("dos draw", true)]
        [Alias("dosdraw", "dos take", "dostake")]
        public async Task Draw()
        {
            await ReplyIfHasMessageAsync(await Game.DrawAsync(Context.User));
        }

        [StartedGameRequired]
        [Command("dos done", true)]
        [Alias("dos end", "dosdone", "dosend")]
        public async Task EndTurn()
        {
            await ReplyIfHasMessageAsync(await Game.EndTurnAsync(Context.User));
        }

        [StartedGameRequired]
        [Command("dos select"), Alias("dosselect", "dos choose", "doschoose")]
        public async Task Select([Remainder] string args)
        {
            await ReplyIfHasMessageAsync(await Game.SelectAsync(Context.User, args));
        }

        [StartedGameRequired]
        [Command("dos match", true), Alias("dosm", "dosmatch")]
        public async Task Match([Remainder] string args)
        {
            await ReplyIfHasMessageAsync(await Game.MatchAsync(Context.User, args));
        }

        [StartedGameRequired]
        [Command("dos add", true), Alias("dosa", "dosadd")]
        public async Task Add([Remainder] string args)
        {
            await ReplyIfHasMessageAsync(await Game.AddToCenterRowAsync(Context.User, args));
        }

        [StartedGameRequired]
        [Command("dos hand", true), Alias("dosh", "doshand")]
        public async Task Hand()
        {
            await ReplyIfHasMessageAsync(await Game.SendHandAsync(Context.User, true));
        }

        [StartedGameRequired]
        [Command("dos table", true), Alias("dost", "dostable")]
        public Task Table() => Game.SendTableToChannel(true);

        [StartedGameRequired]
        [Command("dos!", true)]
        public async Task CallDos() => await ReplyIfHasMessageAsync(await Game.CallDosAsync(Context.User));

        [StartedGameRequired]
        [Command("dos callout", true)]
        public async Task Callout() => await ReplyIfHasMessageAsync(await Game.CalloutAsync(Context.User));
    }
}
