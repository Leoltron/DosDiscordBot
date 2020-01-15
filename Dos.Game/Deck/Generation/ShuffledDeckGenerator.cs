using System;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.Deck.Generation
{
    public class ShuffledDeckGenerator : IDeckGenerator
    {
        private readonly Card[] deck;

        public ShuffledDeckGenerator(Card[] deck) => this.deck = deck;

        public Card[] Generate()
        {
            var copy = new Card[deck.Length];
            Array.Copy(deck, copy, deck.Length);
            copy.Shuffle();
            return copy;
        }
    }
}
