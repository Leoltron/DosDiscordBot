using System.Linq;
using Dos.Game.Deck.Generation;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.State;
using FluentAssertions;
using NUnit.Framework;

namespace Dos.Game.Tests
{
    public class GameTests
    {
        [Test]
        public void PlaySingleGame()
        {
            var green8 = 8.Of(CardColor.Green);
            var deck = green8.Repeat(4).ToArray();
            var game = new Game(new DeterminedDeckGenerator(deck), 1, 2);
            game.CurrentState.Should().BeOfType<MatchingCenterRowState>();
            game.MatchCenterRowCard(0, green8, green8).IsSuccess.Should().BeTrue();
            game.CurrentState.Should().BeOfType<MatchingCenterRowState>();
            game.MatchCenterRowCard(0, green8, green8).IsSuccess.Should().BeTrue();
            game.CurrentState.Should().BeOfType<FinishedGameState>();
        }
    }
}
