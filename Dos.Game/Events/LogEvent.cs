namespace Dos.Game.Events
{
    public class LogEvent : Event
    {
        public string Message { get; }

        public LogEvent(DosGame game, string message) : base(game)
        {
            Message = message;
        }
    }
}
