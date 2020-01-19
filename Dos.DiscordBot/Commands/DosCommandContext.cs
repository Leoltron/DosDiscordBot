using System;
using Discord.Commands;
using Discord.WebSocket;

namespace Dos.DiscordBot.Commands
{
    public class DosCommandContext : SocketCommandContext
    {
        public DosCommandContext(DiscordSocketClient client, SocketUserMessage msg) : base(client, msg)
        {
        }

        public DiscordDosGame DosGame { get; set; }

        public int? NextCommandArgPos { get; set; }
    }
}
