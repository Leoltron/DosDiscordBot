using System.Collections.Generic;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Match;
using Dos.Game.Model;
using Dos.Utils;

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

        protected override Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay)
        {
            if (!(cardsToPlay.Length == 1 || cardsToPlay.Length == 2))
                return Result.Fail($"Expected 1 or 2 cards to match, got {cardsToPlay.Length}");

            var match = target.MatchWith(cardsToPlay);

            if (match == MatchType.NoMatch)
                return Result.Fail($"{target} cannot be matched with {string.Join(" and ", cardsToPlay)}");

            if (!Game.centerRow.Contains(target)) return Result.Fail($"{target} is not present at the Central Row");

            var additional = Game.centerRowAdditional
                                 .Where((e, i) => Game.centerRow[i] == target && e.IsEmpty())
                                 .FirstOrDefault();

            if (additional == null) return Result.Fail($"{target} was already matched");

            var missingCards = cardsToPlay.Where(c => !CurrentPlayerHand.Contains(c)).ToList();
            if (missingCards.Any())
            {
                return Result.Fail($"You don't have {string.Join(" and ", missingCards)}");
            }

            foreach (var card in cardsToPlay) CurrentPlayerHand.Remove(card);

            additional.AddRange(cardsToPlay);

            if (!CurrentPlayerHand.IsEmpty())
                return Game.centerRowAdditional.All(c => c.Any())
                    ? Result.Success(match.DefaultResult().AddText(CurrentPlayerFinishMatching().Message).Message)
                    : Result.Success(match.DefaultResult().Message);
            
            Game.CurrentState = new FinishedGameState(this);
            return Result.Success(match.DefaultResult().AddText($"{CurrentPlayerName} won!").Message);

        }

        protected override Result CurrentPlayerDraw()
        {
            Game.DealCard(CurrentPlayer);
            Game.CurrentState = new AddingToCenterRowState(this, 1);
            return Result.Success("Matched nothing? Draw a card. Now select a card to put to the Center Row.");
        }

        protected override Result CurrentPlayerFinishMatching()
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
            }
            else
            {
                message.Add($"Now put {discardCount} card{(discardCount == 1 ? "" : "s")} to the Center Row");
                Game.CurrentState = new AddingToCenterRowState(this, discardCount);
            }

            return Result.Success(string.Join(" ", message));
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

        protected override Result CurrentPlayerAddCardToCenterRow(Card card) =>
            Result.Fail("Finish matching cards first");
    }
}
