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
        public BaseCurrentPlayerState(Game game) : base(game)
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

            if (!Game.centerRow.Contains(target)) return Result.Fail($"{target} is not present at the Central Row");

            var additional = Game.centerRowAdditional
                                 .Where((e, i) => Game.centerRow[i] == target && e.IsEmpty())
                                 .FirstOrDefault();

            if (additional == null) return Result.Fail($"{target} was already matched");

            var missingCards = cardsToPlay.Where(c => !CurrentPlayerHand.Contains(c)).ToList();
            if (missingCards.Any()) return Result.Fail($"You don't have {string.Join(" and ", missingCards)}");

            foreach (var card in cardsToPlay) CurrentPlayerHand.Remove(card);

            Game.CheckCurrentPlayerForDos();

            additional.AddRange(cardsToPlay);

            var (discardCount, drawCount) = matchType.ToColorMatchBonus();
            if (discardCount != 0) CardsToAdd += discardCount;

            if (drawCount != 0)
                for (var i = 0; i < Game.PlayersCount; i++)
                    if (i != CurrentPlayer)
                        Game.DealCards(i, drawCount, false);

            if (CurrentPlayerHand.IsEmpty())
            {
                Game.CurrentState = new FinishedGameState(this);
                return Result.Success(matchType.DefaultResult()
                                               .AddText(
                                                    $"**{CurrentPlayerName}** won! Total score: **{Game.TotalScore}**")
                                               .Message);
            }

            Game.CurrentState = new BaseCurrentPlayerState(this);

            return Game.centerRowAdditional.All(c => c.Any()) && CardsToAdd == 0
                ? Result.Success(matchType.DefaultResult().AddText(CurrentPlayerEndTurn().Message).Message)
                : Result.Success(matchType.DefaultResult().Message);
        }

        protected override Result CurrentPlayerDraw() =>
            Result.Fail(DrewCard ? "One card is enough for you, play already." : "Too late, you can't draw cards now.");

        protected override Result CurrentPlayerEndTurn()
        {
            ClearMatchedCardsFromCenterRow();
            var refilled = Game.RefillCenterRow();

            if (CardsToAdd > 0 && Game.CurrentPlayerHand.Any())
            {
                var message = $"You need to add **{CardsToAdd}** more {(CardsToAdd == 1 ? "card" : "cards")}";
                if (refilled)
                {
                    Game.CurrentState = new AddingToCenterRowState(this, CardsToAdd);
                    message = "Refilled Center Row with fresh cards. " + message;
                }

                return refilled ? Result.Success(message) : Result.Fail(message);
            }

            Game.MoveTurnToNextPlayer();
            Game.CurrentState = new TurnStartState(this);

            return Result.Success();
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

        protected override Result CurrentPlayerAddCardToCenterRow(Card card)
        {
            ClearMatchedCardsFromCenterRow();
            Game.RefillCenterRow();

            if (!CurrentPlayerHand.Contains(card))
                return Result.Fail($"You don't have {card}");

            CurrentPlayerHand.Remove(card);
            Game.centerRow.Add(card);
            Game.centerRowAdditional.Add(new List<Card>());

            if (CurrentPlayerHand.IsEmpty())
            {
                Game.CurrentState = new FinishedGameState(this);
                return Result.Success($"**{CurrentPlayerName}** won! Total score: **{Game.TotalScore}**");
            }

            Game.CheckCurrentPlayerForDos();

            CardsToAdd--;
            var state = new AddingToCenterRowState(this, CardsToAdd);
            Game.CurrentState = state;

            return CardsToAdd == 0 ? state.CurrentPlayerEndTurn() : Result.Success($"**{CardsToAdd}** more");
        }
    }
}
