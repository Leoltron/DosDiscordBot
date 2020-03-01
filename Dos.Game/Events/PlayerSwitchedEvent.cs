using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class PlayerSwitchedEvent : Event
    {
        public Player PreviousPlayer { get; }
        public Player NextPlayer { get; }

        public PlayerSwitchedEvent(DosGame game, Player previousPlayer, Player nextPlayer) : base(game)
        {
            PreviousPlayer = previousPlayer;
            NextPlayer = nextPlayer;
        }
    }
}
