using System;
using System.Collections.Generic;
using System.Linq;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.Deck
{
    public abstract class DealerBase : Dealer
    {
        protected readonly List<Card> DiscardPile;
        protected readonly List<Card> DrawPile;

        protected DealerBase(IEnumerable<Card> cards)
        {
            DrawPile = new List<Card>(cards);
            DiscardPile = new List<Card>(DrawPile.Count);
        }

        public override bool CanDealCards => DiscardPile.Any() || DrawPile.Any();

        public override void DiscardCards(IEnumerable<Card> cards)
        {
            DiscardPile.AddRange(cards);
        }

        protected virtual void MoveCardsFromDiscardToDraw()
        {
            DrawPile.AddRange(DiscardPile);
            DiscardPile.Clear();
        }

        public override Card DealCard()
        {
            if (DrawPile.Any())
            {
                return DrawCardFromDrawPile();
            }

            if (DiscardPile.IsEmpty())
                throw new InvalidOperationException("Cannot deal card: both discard and draw piles are empty");

            MoveCardsFromDiscardToDraw();
            return DealCard();
        }

        protected virtual Card DrawCardFromDrawPile() => DrawPile.RemoveAtAndReturn(DrawPile.Count - 1);
    }
}
