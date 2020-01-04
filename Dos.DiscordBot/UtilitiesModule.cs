using System.Threading.Tasks;
using Discord.Commands;

namespace Dos.DiscordBot
{
    [Group("dos")]
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        private static readonly string[] Pongs = {"Pong!", "pong", "Ping! I mean, pong!", "...", "Yeah, I'm alive"};

        [Command("ping")]
        public async Task Ping() => await Context.Channel.SendMessageAsync(Pongs.RandomElement());
    }
}
