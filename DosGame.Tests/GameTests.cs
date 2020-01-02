using System.Linq;
using DosGame.Deck.Generation;
using DosGame.Extensions;
using DosGame.Model;
using DosGame.State;
using FluentAssertions;
using NUnit.Framework;

namespace DosGame.Tests
{
    public class GameTests
    {
        [Test]
        public void PlaySingleGame()
        {
            var green8 = 8.Of(CardColor.Green);
            var deck = green8.Repeat(4).ToArray();
            var game = new Game(new DeterminedDeckGenerator(deck), 1, 2);
            game.CurrentState.Should().NotBeOfType<FinishedGameState>();
            game.MatchCenterRowCard(0, green8, green8).IsSuccess.Should().BeTrue();
            game.CurrentState.Should().NotBeOfType<FinishedGameState>();
            game.MatchCenterRowCard(0, green8, green8).IsSuccess.Should().BeTrue();
            game.CurrentState.Should().BeOfType<FinishedGameState>();
        }
    }
}
