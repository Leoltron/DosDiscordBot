using Dos.Game.Model;
using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class CenterRowAddedCardEvent : PlayerEvent
    {
        public Card Card { get; }

        public CenterRowAddedCardEvent(DosGame game, Player player, Card card) : base(game, player)
        {
            Card = card;
        }
    }
}
