namespace Dos.Game.Events
{
    public class Event
    {
        public Event(DosGame game)
        {
            Game = game;
        }

        protected DosGame Game { get; }
    }
}
