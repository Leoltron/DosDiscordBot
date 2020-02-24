using Discord;
using Dos.DiscordBot.Util;
using Dos.Game.Players;

namespace Dos.DiscordBot
{
    public class DiscordUserPlayer : HumanPlayer
    {
        public DiscordUserPlayer(int orderId, IUser user) : base(orderId, user.DiscordTag())
        {
            User = user;
        }

        public IUser User { get; }
    }
}
