using System;
using Discord;
using Discord.WebSocket;

namespace Dos.DiscordBot
{
    public class DiscordDosGameInfo
    {
        public string ServerName { get; }
        public DateTime CreateDate { get; }
        public ISocketMessageChannel Channel { get; }
        public IUser Owner { get; }

        public DiscordDosGameInfo(string serverName, DateTime date, ISocketMessageChannel channel, IUser owner)
        {
            ServerName = serverName;
            CreateDate = date;
            Channel = channel;
            Owner = owner;
        }
    }
}
