using System;
using System.Linq;
using DosGame.Deck;
using DosGame.Deck.Generation;
using DosGame.Model;
using FluentAssertions;
using NUnit.Framework;

namespace DosGame.Tests
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

        [Test]
        public void JustShuffle()
        {
            var deck = new ShuffledDeckGenerator(Decks.Classic).Generate();
            Console.WriteLine();
        }
    }
}
