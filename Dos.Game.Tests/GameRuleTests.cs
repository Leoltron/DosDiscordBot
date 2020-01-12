using System.Linq;
using Dos.Game.Deck;
using Dos.Game.Deck.Generation;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.Tests.Util;
using Dos.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace Dos.Game.Tests
{
    public class GameRuleTests
    {
        private Game game;
        private static readonly Card GreenSharp = CardValue.Sharp.Of(CardColor.Green);
        private static readonly Card[] TwentyGreenSharps = GreenSharp.Repeat(20).ToArray();

        [SetUp]
        public void SetUp()
        {
            game = new Game(new DeterminedDeckGenerator(TwentyGreenSharps), 1, 7);
        }

        [Test]
        public void CanDraw_AtTurnStart()
        {
            game.Draw(0).ShouldBeSuccess();
        }

        [Test]
        public void CanMatch_AtTurnStart()
        {
            game.MatchCenterRowCard(0, GreenSharp, GreenSharp).ShouldBeSuccess();
        }

        [Test]
        public void CannotEndTurn_AtTurnStart()
        {
            game.EndTurn(0).ShouldBeFail();
        }

        [Test]
        public void CanCallDos_AfterHavingTwoCards()
        {
            game = new Game(new DeterminedDeckGenerator(TwentyGreenSharps), 1, 3);
            game.MatchCenterRowCard(0, GreenSharp, GreenSharp);
            
            game.CallDos(0).ShouldBeSuccess();
        }

        [Test]
        public void ShouldNotCallDos_AfterMatchingTwoCardsWhileHavingThree()
        {
            game = new Game(new DeterminedDeckGenerator(TwentyGreenSharps), 1, 3);
            game.MatchCenterRowCard(0, GreenSharp, GreenSharp, GreenSharp);
            
            game.CallDos(0).ShouldBeFail();
        }

        [Test]
        public void ShouldCallDos_AfterTurnEnd()
        {
            game = new Game(new DeterminedDeckGenerator(TwentyGreenSharps), 2, 3) {CurrentPlayer = 0};
            game.MatchCenterRowCard(0, GreenSharp, GreenSharp);
            game.AddCardToCenterRow(0, GreenSharp);
            game.EndTurn(0);

            game.CallDos(0).ShouldBeSuccess();
        }

        [Test]
        public void ShouldCallout_AfterTurnEnd()
        {
            game = new Game(new DeterminedDeckGenerator(TwentyGreenSharps), 2, 3) {CurrentPlayer = 0};
            game.MatchCenterRowCard(0, GreenSharp, GreenSharp);
            game.AddCardToCenterRow(0, GreenSharp);
            game.EndTurn(0);

            game.Callout(1).ShouldBeSuccess();
        }

        [Test]
        public void ShouldNotCallDos_AfterNextPlayerMadeAMove()
        {
            game = new Game(new DeterminedDeckGenerator(TwentyGreenSharps), 2, 3) {CurrentPlayer = 0};
            game.MatchCenterRowCard(0, GreenSharp, GreenSharp);
            game.AddCardToCenterRow(0, GreenSharp);
            game.EndTurn(0);

            game.Draw(1);
            game.CallDos(1).ShouldBeFail();
        }

        [Test]
        public void CantEndTurn_RightAfterDraw()
        {
            game.Draw(game.CurrentPlayer);
            game.EndTurn(game.CurrentPlayer).ShouldBeFail();
        }
    }
}
