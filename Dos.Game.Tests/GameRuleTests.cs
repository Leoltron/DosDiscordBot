using System.Linq;
using Dos.Game.Deck;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.Players;
using Dos.Game.Tests.Util;
using Dos.Utils;
using NUnit.Framework;

namespace Dos.Game.Tests
{
    public class GameRuleTests
    {
        private static readonly Card GreenSharp = CardValue.Sharp.Of(CardColor.Green);
        private static readonly Card[] TwentyGreenSharps = GreenSharp.Repeat(20).ToArray();
        private DosGame game;
        private Player Player0 => game.Players[0];
        private Player Player1 => game.Players[1];

        [SetUp]
        public void SetUp()
        {
            game = new DosGame(new NonShufflingDealer(TwentyGreenSharps), 1, 7);
        }

        [Test]
        public void CanDraw_AtTurnStart()
        {
            game.Draw(Player0).ShouldBeSuccess();
        }

        [Test]
        public void CanMatch_AtTurnStart()
        {
            game.MatchCenterRowCard(Player0, GreenSharp, GreenSharp).ShouldBeSuccess();
        }

        [Test]
        public void CannotEndTurn_AtTurnStart()
        {
            game.EndTurn(Player0).ShouldBeFail();
        }

        [Test]
        public void CanCallDos_AfterHavingTwoCards()
        {
            game = new DosGame(new NonShufflingDealer(TwentyGreenSharps), 1, 3);
            game.MatchCenterRowCard(Player0, GreenSharp, GreenSharp);

            game.CallDos(Player0).ShouldBeSuccess();
        }

        [Test]
        public void ShouldNotCallDos_AfterMatchingTwoCardsWhileHavingThree()
        {
            game = new DosGame(new NonShufflingDealer(TwentyGreenSharps), 1, 3);
            game.MatchCenterRowCard(Player0, GreenSharp, GreenSharp, GreenSharp);

            game.CallDos(Player0).ShouldBeFail();
        }

        [Test]
        public void ShouldCallDos_AfterTurnEnd()
        {
            game = new DosGame(new NonShufflingDealer(TwentyGreenSharps), 2, 3);
            game.MatchCenterRowCard(Player0, GreenSharp, GreenSharp);
            game.AddCardToCenterRow(Player0, GreenSharp);
            game.EndTurn(Player0);

            game.CallDos(Player0).ShouldBeSuccess();
        }

        [Test]
        public void ShouldCallout_AfterTurnEnd()
        {
            game = new DosGame(new NonShufflingDealer(TwentyGreenSharps), 2, 3);
            game.MatchCenterRowCard(Player0, GreenSharp, GreenSharp);
            game.AddCardToCenterRow(Player0, GreenSharp);
            game.EndTurn(Player0);

            game.Callout(Player1, Player0).ShouldBeSuccess();
        }

        [Test]
        public void ShouldNotCallDos_AfterNextPlayerMadeAMove()
        {
            game = new DosGame(new NonShufflingDealer(TwentyGreenSharps), 2, 3);
            game.MatchCenterRowCard(Player0, GreenSharp, GreenSharp);
            game.AddCardToCenterRow(Player0, GreenSharp);
            game.EndTurn(Player0);

            game.Draw(Player1);
            game.CallDos(Player1).ShouldBeFail();
        }

        [Test]
        public void CantEndTurn_RightAfterDraw()
        {
            game.Draw(game.CurrentPlayer);
            game.EndTurn(game.CurrentPlayer).ShouldBeFail();
        }
    }
}
