using System.Collections.Generic;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Match;
using Dos.Game.Model;
using Dos.Game.Players;
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

        public override bool CanMatch => true;
        public override bool CanAdd => CardsToAdd > 0;

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

            var missingCards = cardsToPlay.Where(c => !CurrentPlayer.Hand.Contains(c)).ToList();
            if (missingCards.Any())
                return Result.Fail($"You don't have {string.Join(" and ", missingCards)}");

            foreach (var card in cardsToPlay)
                CurrentPlayer.Hand.Remove(card);

            Game.CheckCurrentPlayerForDos();

            additional.AddRange(cardsToPlay);

            Game.PrivateLog($"{CurrentPlayer} put {string.Join(" and ", cardsToPlay)} to {target}");

            var (discardCount, drawCount) = matchType.ToColorMatchBonus(Game.Config);
            if (discardCount != 0)
                CardsToAdd += discardCount;

            if (drawCount != 0)
                Game.Players
                    .Where(p => p != CurrentPlayer && p.IsActive())
                    .ForEach(p => Game.DealCards(p, drawCount, false));

            Game.MatchCount++;

            Game.CurrentState = new BaseCurrentPlayerState(this);

            var result = matchType.ToResult(Config);

            if (Game.Config.SevenSwap &&
                CurrentPlayer.Hand.Any() &&
                target.Value == CardValue.Seven &&
                matchType.IsColorMatch())
            {
                result = result.AddText("Color match on a 7! Switch your hand with any player.");
                Game.CurrentState = new TriggeredSwapGameState(this);
            }
            
            if (CurrentPlayer.Hand.IsEmpty() || Game.CenterRowAdditional.All(c => c.Any()) && CardsToAdd == 0)
            {
                Game.PublicLog(result.Message);
                Game.CurrentState.EndTurn(CurrentPlayer);
            }
            else
            {
                Game.PublicLog(result.Message);
            }

            return Result.Success();
        }

        protected override Result CurrentPlayerDraw() =>
            Result.Fail(DrewCard ? "You may only draw once." : "You've already made matches, you're not able to draw.");

        protected override Result CurrentPlayerEndTurn()
        {
            Game.ClearMatchedCardsFromCenterRow();
            var refilled = Game.RefillCenterRow();

            if (CardsToAdd > 0 && Game.CurrentPlayer.Hand.Any())
            {
                if (refilled)
                {
                    Game.CurrentState = new AddingToCenterRowState(this, CardsToAdd);
                    Game.PublicLog("Refilled Center Row with fresh cards.");
                }

                Game.PublicLog($"Add **{CardsToAdd}** more {CardsToAdd.PluralizedString("card", "cards")}");

                return new Result(refilled);
            }

            Game.MoveTurnToNextPlayer();
            return Result.Success();
        }

        protected override Result CurrentPlayerAddCardToCenterRow(Card card)
        {
            if (CardsToAdd <= 0)
                return Result.Fail("You can't add cards now.");

            Game.ClearMatchedCardsFromCenterRow();
            Game.RefillCenterRow();

            if (!CurrentPlayer.Hand.Contains(card))
                return Result.Fail($"You don't have {card}");

            CurrentPlayer.Hand.Remove(card);
            Game.CenterRow.Add(card);
            Game.CenterRowAdditional.Add(new List<Card>());

            Game.CheckCurrentPlayerForDos();
            Game.PrivateLog($"{CurrentPlayer} added {card} to the Center Row");
            Game.Events.InvokePlayerAddedCard(CurrentPlayer, card);

            CardsToAdd--;
            var state = new AddingToCenterRowState(this, CardsToAdd);
            Game.CurrentState = state;

            if (CardsToAdd > 0 && Game.CurrentPlayer.Hand.Any())
            {
                Game.PublicLog($"**{CardsToAdd}** more");
            }

            return CardsToAdd == 0 || Game.CurrentPlayer.Hand.IsEmpty()
                ? state.CurrentPlayerEndTurn()
                : Result.Success();
        }

        public override Result SwapWith(Player caller, Player target) => Result.Fail();
    }
}
