using System.Collections.Generic;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Match;
using Dos.Game.Model;
using Dos.Game.Util;

namespace Dos.Game.State
{
    public class MatchingCenterRowState : CurrentPlayerOnlyState
    {
        public MatchingCenterRowState(Game game) : base(game)
        {
        }

        public MatchingCenterRowState(GameState gameState) : base(gameState)
        {
        }

        protected override Result<string> CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay)
        {
            if (!(cardsToPlay.Length == 1 || cardsToPlay.Length == 2))
                return $"Expected 1 or 2 cards to match, got {cardsToPlay.Length}".ToFail();

            var match = target.MatchWith(cardsToPlay);

            if (match != MatchType.NoMatch)
            {
                if (!Game.centerRow.Contains(target)) return $"{target} is not present at the Central Row".ToFail();

                var additional = Game.centerRowAdditional
                                     .Where((e, i) => Game.centerRow[i] == target && e.IsEmpty())
                                     .FirstOrDefault();

                if (additional == null) return $"{target} was already matched".ToFail();

                if (!CurrentPlayerHand.Contains(cardsToPlay)) return "You don't have specified cards".ToFail();

                foreach (var card in cardsToPlay) CurrentPlayerHand.Remove(card);

                additional.AddRange(cardsToPlay);

                if (CurrentPlayerHand.IsEmpty())
                {
                    Game.CurrentState = new FinishedGameState(this);
                    return match.DefaultResult().AddText($"{CurrentPlayerName} won!").Message.ToSuccess();
                }

                return match.DefaultResult().Message.ToSuccess();
            }

            return $"{target} cannot be matched with {string.Join(" and ", cardsToPlay)}".ToFail();
        }

        protected override Result<string> CurrentPlayerDraw()
        {
            Game.DealCard(CurrentPlayer);
            Game.CurrentState = new AddingToCenterRowState(this, 1);
            return "Matched nothing? Draw a card. Now select a card to put to the Center Row.".ToSuccess();
        }

        protected override Result<string> CurrentPlayerFinishMatching()
        {
            if (Game.centerRowAdditional.SelectMany(c => c).IsEmpty()) return CurrentPlayerDraw();

            var discardCount = CountBonusFromMatches(out var drawCount);
            var message = new List<string>();
            if (drawCount != 0)
            {
                for (var i = 0; i < Game.PlayersCount; i++)
                    if (i != CurrentPlayer)
                        Game.DealCards(i, drawCount);

                var matchName = drawCount == 1
                    ? "Double Color Match!"
                    : $"{drawCount} Double Color Matches" + string.Join("", "!".Repeat(drawCount));

                message.Add($"{matchName} Everyone else, draw {drawCount}");
            }

            ClearMatchedCardsFromCenterRow();

            Game.EnsureCenterRowIsValid();
            if (discardCount <= 0)
            {
                Game.MoveTurnToNextPlayer();
                message.Add("Now it's your turn, " + Game.GetPlayerName(CurrentPlayer));
            }
            else
            {
                message.Add($"Now put {discardCount} card{(discardCount == 1 ? "" : "s")} to the Center Row");
                Game.CurrentState = new AddingToCenterRowState(this, discardCount);
            }

            return string.Join(" ", message).ToSuccess();
        }

        private void ClearMatchedCardsFromCenterRow()
        {
            for (var i = 0; i < Game.centerRow.Count; i++)
            {
                if (Game.centerRowAdditional[i].IsEmpty()) continue;
                Game.discardPile.Push(Game.centerRow[i]);
                Game.centerRow.RemoveAt(i);
                Game.centerRowAdditional[i].ForEach(c => Game.discardPile.Push(c));
                Game.centerRowAdditional.RemoveAt(i);
                i--;
            }
        }

        private int CountBonusFromMatches(out int drawCount)
        {
            int discardCount;
            (discardCount, drawCount) = Game
                                       .centerRow.Select((c, i) => c.MatchWith(Game.centerRowAdditional[i].ToArray()))
                                       .Select(m => m.ToColorMatchBonus())
                                       .Aggregate((discardCount: 0, drawCount: 0),
                                                  (prev, bonus) => (
                                                      prev.discardCount + bonus.discardCount,
                                                      prev.drawCount + bonus.drawCount));
            return discardCount;
        }

        protected override Result<string> CurrentPlayerAddCardToCenterRow(Card card) =>
            "Finish matching cards first".ToFail();
    }
}
