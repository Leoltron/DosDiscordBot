using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.Commands;
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
            await ReplyIfHasMessageAsync(await gameRouterService.JoinGameAsync(Context.Guild, Context.Channel, Context.User));
        }

        [Command("dos start", true), Alias("dosstart", "doss")]
        public async Task StartGame()
        {
            await ReplyIfHasMessageAsync(await Game.StartAsync(Context.User));
        }

        [RunningGameRequired]
        [Command("dos draw", true)]
        [Alias("dosdraw", "dosd", "dos take", "dos pick", "dos done", "dostake", "dospick", "dosdone")]
        public async Task FinishMatching()
        {
            await ReplyIfHasMessageAsync(await Game.FinishMatchingAsync(Context.User));
        }

        [RunningGameRequired]
        [Command("dos select"), Alias("dosselect", "dos choose", "doschoose")]
        public async Task Select([Remainder] string args)
        {
            await ReplyIfHasMessageAsync(await Game.SelectAsync(Context.User, args));
        }

        [RunningGameRequired]
        [Command("dos play", true), Alias("dosp", "dosplay")]
        public async Task Play([Remainder] string args)
        {
            await ReplyIfHasMessageAsync(await Game.PlayAsync(Context.User, args));
        }

        [RunningGameRequired]
        [Command("dos hand", true), Alias("dosh", "doshand")]
        public async Task Hand()
        {
            await ReplyIfHasMessageAsync(await Game.SendHandAsync(Context.User, true));
        }

        [RunningGameRequired]
        [Command("dos table", true), Alias("dost", "dostable")]
        public Task Table() => ReplyIfHasMessageAsync(Game.GetTable(true));
    }
}
