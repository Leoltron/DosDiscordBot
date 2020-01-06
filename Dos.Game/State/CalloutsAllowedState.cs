using Dos.Utils;

namespace Dos.Game.State
{
    public abstract class CalloutsAllowedState : GameState
    {
        protected CalloutsAllowedState(Game game) : base(game)
        {
        }

        protected CalloutsAllowedState(GameState gameState) : base(gameState)
        {
        }

        public override Result Callout(int caller)
        {
            if (!Game.CurrentPlayerDidNotCallDos)
                return Game.FalseCalloutPenalty > 0
                    ? Result.Success($"False callout! {Game.GetPlayerName(caller)}, draw {Game.FalseCalloutPenalty}.")
                    : Result.Fail("False callout!");


            Game.CurrentPlayerDidNotCallDos = false;

            if (Game.CalloutPenalty <= 0)
                return Result.Success($"You are right, {CurrentPlayerName} did not call DOS but there is no penalty");

            Game.CurrentPlayerPenalty += Game.CalloutPenalty;
            return Result.Success($"{CurrentPlayerName}, you have been caught not calling DOS with two cards " +
                                   $"in hand! Draw {Game.CurrentPlayerPenalty} when your turn ends.");
        }

        public override Result CallDos(int caller)
        {
            if (caller != CurrentPlayer)
                return Result.Fail("You do not need to call DOS if you got 2 cards outside of your turn or you already finished it.");

            var cardsCount = Game.playerHands[caller].Count;
            if (cardsCount != 2 && !Game.CurrentPlayerDidNotCallDos)
                return Result.Fail($"You do not have to call DOS since you have {cardsCount} cards, not 2");

            Game.CurrentPlayerDidNotCallDos = false;
            return Result.Success($"**DOS! {Game.GetPlayerName(caller)} has only 2 cards!**");
        }
    }
}
