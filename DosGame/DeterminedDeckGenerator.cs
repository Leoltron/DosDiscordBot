using System;

namespace DosGame
{
    public class DeterminedDeckGenerator : IDeckGenerator
    {
        private readonly Card[] deck;

        public DeterminedDeckGenerator(Card[] deck)
        {
            this.deck = deck;
        }

        public Card[] Generate()
        {
            var copy = new Card[deck.Length];
            Array.Copy(deck, copy, deck.Length);
            return copy;
        }
    }
}
