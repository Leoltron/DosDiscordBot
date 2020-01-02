using System.Linq;
using DosGame.Extensions;
using DosGame.Model;

namespace DosGame.Deck
{
    public static class Decks
    {
        public static readonly Card[] Classic = new[] {1, 3, 4, 5}.OfAllColors().Times(3)
                                                                  .Concat(new[] {6, 7, 8, 9, 10}.OfAllColors().Times(2))
                                                                  .Concat(CardValue.Sharp.OfAllColors().Times(2))
                                                                  .Concat(Card.WildDos.Repeat(12))
                                                                  .ToArray();
    }
}
