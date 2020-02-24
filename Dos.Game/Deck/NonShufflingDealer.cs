using System.Collections.Generic;
using Dos.Game.Model;

namespace Dos.Game.Deck
{
    public class NonShufflingDealer : DealerBase
    {
        public NonShufflingDealer(IEnumerable<Card> cards) : base(cards)
        {
        }

        protected override void MoveCardsFromDiscardToDraw()
        {
            DiscardPile.Reverse();
            DrawPile.AddRange(DiscardPile);
            DiscardPile.Clear();
        }
    }
}
