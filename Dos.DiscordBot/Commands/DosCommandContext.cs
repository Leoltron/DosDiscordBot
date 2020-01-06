using Discord.Commands;
using Discord.WebSocket;

namespace Dos.DiscordBot.Commands
{
    public class DosCommandContext : SocketCommandContext
    {
        public DiscordDosGame DosGame { get; set; }

        public DosCommandContext(DiscordSocketClient client, SocketUserMessage msg) : base(client, msg)
        {
        }
    }
}
