using System.Threading.Tasks;
using Discord.Commands;

namespace Dos.DiscordBot
{
    // ReSharper disable once UnusedType.Global
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameProviderService gameProviderService;

        public GameModule(GameProviderService gameProviderService)
        {
            this.gameProviderService = gameProviderService;
        }

        [Command("dos join", true), Alias("dosj")]
        public async Task JoinGame()
        {
            var result = await gameProviderService.JoinGameAsync(Context.Channel, Context.User);
            await ReplyAsync(result.Value);
        }

        [Command("dos start", true), Alias("doss")]
        public async Task StartGame()
        {
            var result = await gameProviderService.StartGameAsync(Context.Channel, Context.User);
            await ReplyAsync(result.Value);
        }
    }
}
