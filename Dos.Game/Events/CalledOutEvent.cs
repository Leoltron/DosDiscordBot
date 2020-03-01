using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class CalledOutEvent : Event
    {
        public Player Caller { get; }
        public Player Target { get; }

        public CalledOutEvent(DosGame game, Player caller, Player target) : base(game)
        {
            Caller = caller;
            Target = target;
        }
    }
}
