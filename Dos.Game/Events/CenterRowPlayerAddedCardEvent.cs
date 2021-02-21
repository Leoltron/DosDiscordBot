using Dos.Game.Model;
using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class CenterRowPlayerAddedCardEvent : PlayerEvent
    {
        public Card Card { get; }

        public CenterRowPlayerAddedCardEvent(DosGame game, Player player, Card card) : base(game, player)
        {
            Card = card;
        }
    }
}
