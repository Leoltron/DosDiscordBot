using System;
using System.Collections.Generic;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.Deck
{
    public class ShufflingDealer : DealerBase
    {
        private readonly Random random = new Random();

        public ShufflingDealer(IEnumerable<Card> cards) : base(cards)
        {
        }

        protected override Card DrawCardFromDrawPile() => DrawPile.RemoveAtAndReturn(random.Next(DrawPile.Count));
    }
}
