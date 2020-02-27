using System.Collections.Generic;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Match;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.State.Base
{
    public class BaseCurrentPlayerState : CurrentPlayerOnlyState
    {
        public BaseCurrentPlayerState(DosGame game) : base(game)
        {
        }

        public BaseCurrentPlayerState(GameState gameState) : base(gameState)
        {
        }

        public BaseCurrentPlayerState(BaseCurrentPlayerState gameState) : this((GameState) gameState)
        {
            DrewCard = gameState.DrewCard;
            CardsToAdd = gameState.CardsToAdd;
        }

        public bool DrewCard { get; set; }
        public int CardsToAdd { get; set; }

        protected override Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay)
        {
            if (!(cardsToPlay.Length == 1 || cardsToPlay.Length == 2))
                return Result.Fail($"Expected 1 or 2 cards to match, got {cardsToPlay.Length}");

            var matchType = target.MatchWith(cardsToPlay);

            if (matchType == MatchType.NoMatch)
                return Result.Fail($"{target} cannot be matched with {string.Join(" and ", cardsToPlay)}");

            if (!Game.CenterRow.Contains(target))
                return Result.Fail($"{target} is not present at the Center Row");

            var additional = Game.CenterRowAdditional
                                 .Where((e, i) => Game.CenterRow[i] == target && e.IsEmpty())
                                 .FirstOrDefault();

            if (additional == null)
                return Result.Fail($"{target} was already matched");

            var missingCards = cardsToPlay.Where(c => !CurrentPlayerHand.Contains(c)).ToList();
            if (missingCards.Any())
                return Result.Fail($"You don't have {string.Join(" and ", missingCards)}");

            foreach (var card in cardsToPlay)
                CurrentPlayerHand.Remove(card);

            Game.CheckCurrentPlayerForDos();

            additional.AddRange(cardsToPlay);

            var (discardCount, drawCount) = matchType.ToColorMatchBonus();
            if (discardCount != 0)
                CardsToAdd += discardCount;

            if (drawCount != 0)
                Game.Players
                    .Where(p => p != CurrentPlayer)
                    .ForEach(p => Game.DealCards(p, drawCount, false));

            Game.MatchCount++;

            Game.CurrentState = new BaseCurrentPlayerState(this);

            return CurrentPlayerHand.IsEmpty() || Game.CenterRowAdditional.All(c => c.Any()) && CardsToAdd == 0
                ? Result.Success(matchType.DefaultResult().AddText(CurrentPlayerEndTurn().Message).Message)
                : Result.Success(matchType.DefaultResult().Message);
        }

        protected override Result CurrentPlayerDraw() =>
            Result.Fail(DrewCard ? "You may only draw once." : "You've already made matches, you're not able to draw.");

        protected override Result CurrentPlayerEndTurn()
        {
            ClearMatchedCardsFromCenterRow();
            var refilled = Game.RefillCenterRow();

            if (CardsToAdd > 0 && Game.CurrentPlayerHand.Any())
            {
                var message = $"Now add **{CardsToAdd}** more {(CardsToAdd == 1 ? "card" : "cards")}";
                if (refilled)
                {
                    Game.CurrentState = new AddingToCenterRowState(this, CardsToAdd);
                    message = "Refilled Center Row with fresh cards. " + message;
                }

                return refilled ? Result.Success(message) : Result.Fail(message);
            }

            var currentPlayer = CurrentPlayer;
            Game.MoveTurnToNextPlayer();
            if (!(Game.CurrentState is FinishedGameState))
                Game.CurrentState = new TurnStartState(this);

            return Result.Success(currentPlayer.ScoreBoardPosition
                                               .IfHasValue(i => $"{currentPlayer.Name} has no more cards! " +
                                                                $"They finished in Rank #{i}! :tada:"));
        }

        private void ClearMatchedCardsFromCenterRow()
        {
            for (var i = 0; i < Game.CenterRow.Count; i++)
            {
                if (Game.CenterRowAdditional[i].IsEmpty())
                    continue;
                Game.Dealer.DiscardCard(Game.CenterRow[i]);
                Game.CenterRow.RemoveAt(i);
                Game.Dealer.DiscardCards(Game.CenterRowAdditional[i]);
                Game.CenterRowAdditional.RemoveAt(i);
                i--;
            }
        }

        protected override Result CurrentPlayerAddCardToCenterRow(Card card)
        {
            if (CardsToAdd <= 0)
                return Result.Fail("You can't add cards now.");

            ClearMatchedCardsFromCenterRow();
            Game.RefillCenterRow();

            if (!CurrentPlayerHand.Contains(card))
                return Result.Fail($"You don't have {card}");

            CurrentPlayerHand.Remove(card);
            Game.CenterRow.Add(card);
            Game.CenterRowAdditional.Add(new List<Card>());

            Game.CheckCurrentPlayerForDos();

            CardsToAdd--;
            var state = new AddingToCenterRowState(this, CardsToAdd);
            Game.CurrentState = state;

            return CardsToAdd == 0 ? state.CurrentPlayerEndTurn() : Result.Success($"**{CardsToAdd}** more");
        }
    }
}
