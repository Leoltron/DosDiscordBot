using Dos.Game.Model;
using Dos.Game.Players;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game.State
{
    public class TriggeredSwapGameState : BaseCurrentPlayerState
    {
        private static readonly Result SwappingResult =
            Result.Fail("You have to swap hands with other player.");

        public TriggeredSwapGameState(BaseCurrentPlayerState gameState) : base(gameState)
        {
        }

        public override bool CanMatch => false;
        public override bool CanAdd => false;
        public override bool CanDraw => false;

        public override Result SwapWith(Player caller, Player target)
        {
            if (caller != CurrentPlayer || caller == target)
            {
                return Result.Fail();
            }

            var prevCallerHand = caller.Hand;
            caller.Hand = target.Hand;
            target.Hand = prevCallerHand;
            Game.CheckCurrentPlayerForDos();

            Game.PrivateLog($"{caller} swapped hands with {target}");
            Game.Events.InvokePlayersSwappedHands(caller, target);
            Game.CurrentState = new BaseCurrentPlayerState(this);

            return Result.Success($"Ok, now you have {target}'s cards, continue matching");
        }

        protected override Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay) => SwappingResult;

        protected override Result CurrentPlayerDraw() => SwappingResult;

        protected override Result CurrentPlayerEndTurn() => SwappingResult;

        protected override Result CurrentPlayerAddCardToCenterRow(Card card) => SwappingResult;
    }
}
