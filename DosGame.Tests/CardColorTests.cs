using DosGame.Model;
using FluentAssertions;
using NUnit.Framework;

namespace DosGame.Tests
{
    public class CardColorTests
    {
        [TestCase(CardColor.Blue)]
        [TestCase(CardColor.Green)]
        [TestCase(CardColor.Red)]
        [TestCase(CardColor.Yellow)]
        public void EqualColorMatches(CardColor color)
        {
            color.Matches(color).Should().BeTrue();
        }

        [Test]
        public void DifferentColorDoNotMatch()
        {
            CardColor.Blue.Matches(CardColor.Green).Should().BeFalse();
        }

        [TestCase(CardColor.Blue)]
        [TestCase(CardColor.Green)]
        [TestCase(CardColor.Red)]
        [TestCase(CardColor.Yellow)]
        [TestCase(CardColor.Wild)]
        public void EverythingMatchesWild(CardColor color)
        {
            CardColor.Wild.Matches(color).Should().BeTrue();
        }
    }
}
