using System.Linq;
using Dos.Game.Deck;
using Dos.Game.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Dos.Game.Tests
{
    public class DeckTests
    {
        [Test]
        public void TestDeckCount()
        {
            Decks.Classic.Length.Should().Be(108);
        }

        [Test]
        public void TestDeckNumberedColoredCount()
        {
            Decks.Classic.Count(c => c.Color != CardColor.Wild).Should().Be(96);
        }
    }
}
