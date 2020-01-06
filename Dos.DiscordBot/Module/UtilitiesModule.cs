using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.Commands;
using Dos.Utils;

namespace Dos.DiscordBot.Module
{
    [Group("dos")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        private static readonly string[] Pongs = {"Pong!", "pong", "Ping! I mean, pong!", "...", "Yeah, I'm alive"};

        [Command("ping")]
        public async Task Ping() => await Context.Channel.SendMessageAsync(Pongs.RandomElement());
    }
}
