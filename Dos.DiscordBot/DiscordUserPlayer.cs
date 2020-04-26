using Discord;
using Dos.DiscordBot.Util;
using Dos.Game.Players;

namespace Dos.DiscordBot
{
    public class DiscordUserPlayer : HumanPlayer
    {
        public DiscordUserPlayer(int orderId, IUser user) : base(orderId, GetNameSafely(user))
        {
            User = user;
        }

        private static string GetNameSafely(IUser user)
        {
            var nick = user.GuildDiscordTag();
            return string.IsNullOrWhiteSpace(nick) ? user.Username : nick;
        }

        public IUser User { get; }
    }
}
