using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class PlayerEvent : Event
    {
        public PlayerEvent(DosGame game, Player player) : base(game)
        {
            Player = player;
        }

        public Player Player { get; }
    }
}
