using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class PlayersSwappedHandsEvent : PlayerEvent
    {
        public Player Target { get; }

        public PlayersSwappedHandsEvent(DosGame game, Player player, Player target) : base(game, player)
        {
            Target = target;
        }
    }
}
