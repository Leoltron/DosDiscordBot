using System.Collections.Generic;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.State
{
    public class AddingToCenterRowState : CurrentPlayerOnlyState
    {
        private static readonly Result MatchingFail = Result.Fail("You are already finished matching cards");
        private int cardsToAdd;

        public AddingToCenterRowState(GameState gameState, int cardsToAdd) : base(gameState) =>
            this.cardsToAdd = cardsToAdd;

        protected override Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay) =>
            MatchingFail;

        protected override Result CurrentPlayerDraw() => MatchingFail;

        protected override Result CurrentPlayerFinishMatching() => MatchingFail;

        protected override Result CurrentPlayerAddCardToCenterRow(Card card)
        {
            if (!CurrentPlayerHand.Contains(card))
                return Result.Fail($"You do not have {card}");

            CurrentPlayerHand.Remove(card);
            Game.centerRow.Add(card);
            Game.centerRowAdditional.Add(new List<Card>());

            if (CurrentPlayerHand.IsEmpty())
            {
                Game.CurrentState = new FinishedGameState(this);
                return Result.Success($"{CurrentPlayerName} won!");
            }

            Game.CheckCurrentPlayerForDos();

            cardsToAdd--;
            if (cardsToAdd != 0)
                return Result.Success($"{cardsToAdd} more");

            Game.MoveTurnToNextPlayer();
            Game.CurrentState = new MatchingCenterRowState(this);
            return Result.Success();
        }
    }
}
