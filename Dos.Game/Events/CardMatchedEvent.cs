using Dos.Game.Model;
using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class CardMatchedEvent : PlayerEvent
    {
        public Card Target { get; }
        public Card[] MatchingCards { get; }

        public CardMatchedEvent(DosGame game, Player player, Card target, Card[] matchingCards) : base(game, player)
        {
            Target = target;
            MatchingCards = matchingCards;
        }
    }
}
