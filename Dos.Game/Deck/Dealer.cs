using System.Collections.Generic;
using System.Linq;
using Dos.Game.Model;

namespace Dos.Game.Deck
{
    public abstract class Dealer
    {
        public abstract Card DealCard();
        public abstract void DiscardCards(IEnumerable<Card> cards);

        public void DiscardCard(Card card)
        {
            DiscardCards(card);
        }

        public void DiscardCards(params Card[] cards)
        {
            DiscardCards(cards.AsEnumerable());
        }

        public abstract bool CanDealCards { get; }
    }
}
