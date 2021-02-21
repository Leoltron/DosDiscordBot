using Dos.Game.Model;

namespace Dos.Game.Events
{
    public class CenterRowRefilledEvent : Event
    {
        public Card[] Cards { get; }

        public CenterRowRefilledEvent(DosGame game, Card[] cards) : base(game)
        {
            Cards = cards;
        }
    }
}
