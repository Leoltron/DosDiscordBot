using System.Collections.Generic;
using Dos.Game.Model;
using Dos.Game.Util;
using Dos.Game.Extensions;

namespace Dos.Game.State
{
    public class AddingToCenterRowState : CurrentPlayerOnlyState
    {
        private static readonly Result<string> MatchingFail = "You are already finished matching cards".ToFail();
        private readonly int cardsToAdd;

        public AddingToCenterRowState(GameState gameState, int cardsToAdd) : base(gameState)
        {
            this.cardsToAdd = cardsToAdd;
        }

        protected override Result<string> CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay) =>
            MatchingFail;

        protected override Result<string> CurrentPlayerDraw() => MatchingFail;

        protected override Result<string> CurrentPlayerFinishMatching() => MatchingFail;

        protected override Result<string> CurrentPlayerAddCardToCenterRow(Card card)
        {
            if (!CurrentPlayerHand.Contains(card))
                return $"You do not have {card}".ToFail();

            CurrentPlayerHand.Remove(card);
            Game.centerRow.Add(card);
            Game.centerRowAdditional.Add(new List<Card>());

            if (CurrentPlayerHand.IsEmpty())
            {
                Game.CurrentState = new FinishedGameState(this);
                return $"{CurrentPlayerName} won!".ToSuccess();
            }
            
            Game.CheckCurrentPlayerForDos();

            if (cardsToAdd != 0)
                return $"{cardsToAdd} more".ToSuccess();

            Game.MoveTurnToNextPlayer();
            Game.CurrentState = new MatchingCenterRowState(this);
            return ("Now it's your turn, " + CurrentPlayerName).ToSuccess();
        }
    }
}
